using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.Event;
using GameFramework.Log;
using Sudoku.Data;
using Sudoku.Generator;

namespace Sudoku.Logic
{
    /// <summary>
    /// 数独核心逻辑管理器。
    /// 
    /// 职责：管理棋盘状态、处理填数/擦除/笔记/提示/撤销操作、冲突检测、完成判定。
    /// 依赖：EventSystem、SudokuGenerator
    /// 使用：SudokuManager.Instance.StartNewGame(Difficulty.Medium);
    /// </summary>
    public class SudokuManager : Singleton<SudokuManager>
    {
        private const string TAG = "SudokuManager";

        /// <summary>
        /// 当前棋盘数据
        /// </summary>
        public SudokuBoardData Board { get; private set; }

        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public GameState CurrentState { get; private set; } = GameState.Menu;

        /// <summary>
        /// 是否处于笔记模式
        /// </summary>
        public bool IsNoteMode { get; private set; }

        /// <summary>
        /// 当前选中的格子（行,列），(-1,-1) 表示无选择
        /// </summary>
        public (int Row, int Col) SelectedCell { get; private set; } = (-1, -1);

        // 撤销/重做栈
        private readonly Stack<MoveCommand> _undoStack = new Stack<MoveCommand>();
        private readonly Stack<MoveCommand> _redoStack = new Stack<MoveCommand>();

        /// <summary>
        /// 是否可以撤销
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>
        /// 是否可以重做
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        #region 游戏流程

        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame(Difficulty difficulty)
        {
            GameLogger.LogInfo(TAG, $"Starting new game: {difficulty}");

            var (puzzle, solution) = SudokuGenerator.Generate(difficulty);

            Board = new SudokuBoardData { Difficulty = difficulty };

            for (int r = 0; r < SudokuBoardData.SIZE; r++)
            {
                for (int c = 0; c < SudokuBoardData.SIZE; c++)
                {
                    int idx = r * SudokuBoardData.SIZE + c;
                    var cell = Board.GetCell(r, c);
                    cell.Value = puzzle[idx];
                    cell.IsFixed = puzzle[idx] != 0;
                    Board.SetSolution(r, c, solution[idx]);
                }
            }

            _undoStack.Clear();
            _redoStack.Clear();
            IsNoteMode = false;
            SelectedCell = (-1, -1);
            CurrentState = GameState.Playing;

            EventSystem.Instance.Publish(SudokuEvents.BOARD_CREATED, this);
            EventSystem.Instance.Publish(SudokuEvents.GAME_STARTED, this);
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void Pause()
        {
            if (CurrentState != GameState.Playing) return;
            CurrentState = GameState.Paused;
            EventSystem.Instance.Publish(SudokuEvents.GAME_PAUSED, this);
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public void Resume()
        {
            if (CurrentState != GameState.Paused) return;
            CurrentState = GameState.Playing;
            EventSystem.Instance.Publish(SudokuEvents.GAME_RESUMED, this);
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void ReturnToMenu()
        {
            CurrentState = GameState.Menu;
            SelectedCell = (-1, -1);
        }

        #endregion

        #region 选择

        /// <summary>
        /// 选择格子
        /// </summary>
        public void SelectCell(int row, int col)
        {
            if (CurrentState != GameState.Playing) return;

            SelectedCell = (row, col);
            EventSystem.Instance.Publish(SudokuEvents.CELL_SELECTED, this);
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        public void DeselectCell()
        {
            SelectedCell = (-1, -1);
            EventSystem.Instance.Publish(SudokuEvents.CELL_DESELECTED, this);
        }

        #endregion

        #region 填数/擦除

        /// <summary>
        /// 在当前选中格子填入数字
        /// </summary>
        public void SetNumber(int number)
        {
            if (CurrentState != GameState.Playing) return;
            if (SelectedCell.Row < 0) return;

            var cell = Board.GetCell(SelectedCell.Row, SelectedCell.Col);
            if (cell.IsFixed) return;

            if (IsNoteMode)
            {
                ToggleNote(number);
                return;
            }

            // 记录撤销
            var cmd = new MoveCommand(
                cell.Row, cell.Col,
                cell.Value, number,
                cell.CloneNotes(), new bool[9],
                false
            );

            cell.Value = number;
            cell.ClearNotes();

            _undoStack.Push(cmd);
            _redoStack.Clear();

            // 清除同行/列/宫中其他格子的对应笔记
            RemoveRelatedNotes(cell.Row, cell.Col, number);

            // 冲突检测
            UpdateErrors();

            EventSystem.Instance.Publish(SudokuEvents.CELL_VALUE_CHANGED, this);

            // 检测完成
            CheckCompletion();
        }

        /// <summary>
        /// 擦除当前选中格子
        /// </summary>
        public void ClearCell()
        {
            if (CurrentState != GameState.Playing) return;
            if (SelectedCell.Row < 0) return;

            var cell = Board.GetCell(SelectedCell.Row, SelectedCell.Col);
            if (cell.IsFixed || cell.IsEmpty) return;

            var cmd = new MoveCommand(
                cell.Row, cell.Col,
                cell.Value, 0,
                cell.CloneNotes(), cell.CloneNotes(),
                false
            );

            cell.Value = 0;
            _undoStack.Push(cmd);
            _redoStack.Clear();

            UpdateErrors();
            EventSystem.Instance.Publish(SudokuEvents.CELL_VALUE_CHANGED, this);
        }

        #endregion

        #region 笔记

        /// <summary>
        /// 切换笔记模式
        /// </summary>
        public void ToggleNoteMode()
        {
            IsNoteMode = !IsNoteMode;
            EventSystem.Instance.Publish(SudokuEvents.NOTE_MODE_TOGGLED, this);
        }

        /// <summary>
        /// 在当前选中格子切换笔记数字
        /// </summary>
        private void ToggleNote(int number)
        {
            if (SelectedCell.Row < 0) return;

            var cell = Board.GetCell(SelectedCell.Row, SelectedCell.Col);
            if (cell.IsFixed || !cell.IsEmpty) return;

            var oldNotes = cell.CloneNotes();
            int noteIdx = number - 1;
            cell.Notes[noteIdx] = !cell.Notes[noteIdx];
            var newNotes = cell.CloneNotes();

            var cmd = new MoveCommand(
                cell.Row, cell.Col,
                0, 0,
                oldNotes, newNotes,
                true
            );
            _undoStack.Push(cmd);
            _redoStack.Clear();

            EventSystem.Instance.Publish(SudokuEvents.CELL_NOTES_CHANGED, this);
        }

        /// <summary>
        /// 填入数字后，自动清除同行/列/宫中其他格子的对应笔记
        /// </summary>
        private void RemoveRelatedNotes(int row, int col, int number)
        {
            int noteIdx = number - 1;
            int boxRow = (row / SudokuBoardData.BOX_SIZE) * SudokuBoardData.BOX_SIZE;
            int boxCol = (col / SudokuBoardData.BOX_SIZE) * SudokuBoardData.BOX_SIZE;

            for (int i = 0; i < SudokuBoardData.SIZE; i++)
            {
                // 同行
                Board.GetCell(row, i).Notes[noteIdx] = false;
                // 同列
                Board.GetCell(i, col).Notes[noteIdx] = false;
            }

            // 同宫
            for (int r = boxRow; r < boxRow + SudokuBoardData.BOX_SIZE; r++)
            {
                for (int c = boxCol; c < boxCol + SudokuBoardData.BOX_SIZE; c++)
                {
                    Board.GetCell(r, c).Notes[noteIdx] = false;
                }
            }
        }

        #endregion

        #region 提示

        /// <summary>
        /// 使用一次提示（自动填入一个正确数字）
        /// </summary>
        public bool UseHint()
        {
            if (CurrentState != GameState.Playing) return false;
            if (Board.HintsRemaining <= 0) return false;

            // 优先提示选中的空格，否则找任意一个空格
            int hintRow = -1, hintCol = -1;

            if (SelectedCell.Row >= 0)
            {
                var selCell = Board.GetCell(SelectedCell.Row, SelectedCell.Col);
                if (!selCell.IsFixed && selCell.IsEmpty)
                {
                    hintRow = SelectedCell.Row;
                    hintCol = SelectedCell.Col;
                }
            }

            if (hintRow < 0)
            {
                // 找任意空格
                for (int r = 0; r < SudokuBoardData.SIZE && hintRow < 0; r++)
                {
                    for (int c = 0; c < SudokuBoardData.SIZE; c++)
                    {
                        var cell = Board.GetCell(r, c);
                        if (!cell.IsFixed && cell.IsEmpty)
                        {
                            hintRow = r;
                            hintCol = c;
                            break;
                        }
                    }
                }
            }

            if (hintRow < 0) return false;

            Board.HintsRemaining--;

            var hintCell = Board.GetCell(hintRow, hintCol);
            int answer = Board.GetSolution(hintRow, hintCol);

            var cmd = new MoveCommand(
                hintRow, hintCol,
                hintCell.Value, answer,
                hintCell.CloneNotes(), new bool[9],
                false
            );

            hintCell.Value = answer;
            hintCell.IsFixed = true; // 提示的数字也固定
            hintCell.ClearNotes();

            _undoStack.Push(cmd);
            _redoStack.Clear();

            RemoveRelatedNotes(hintRow, hintCol, answer);
            UpdateErrors();

            EventSystem.Instance.Publish(SudokuEvents.HINT_USED, this);
            EventSystem.Instance.Publish(SudokuEvents.CELL_VALUE_CHANGED, this);

            CheckCompletion();
            return true;
        }

        #endregion

        #region 撤销/重做

        /// <summary>
        /// 撤销上一步操作
        /// </summary>
        public void Undo()
        {
            if (!CanUndo || CurrentState != GameState.Playing) return;

            var cmd = _undoStack.Pop();
            var cell = Board.GetCell(cmd.Row, cmd.Col);

            cell.Value = cmd.OldValue;
            System.Array.Copy(cmd.OldNotes, cell.Notes, 9);
            cell.IsFixed = false; // 撤销后不再固定（除非原始题目）

            // 恢复原始固定状态：如果答案中这个位置是预置的
            if (cmd.OldValue != 0)
            {
                // 保持非固定，让用户可以继续修改
            }

            _redoStack.Push(cmd);
            UpdateErrors();

            EventSystem.Instance.Publish(SudokuEvents.UNDO_PERFORMED, this);
            EventSystem.Instance.Publish(SudokuEvents.CELL_VALUE_CHANGED, this);
        }

        /// <summary>
        /// 重做
        /// </summary>
        public void Redo()
        {
            if (!CanRedo || CurrentState != GameState.Playing) return;

            var cmd = _redoStack.Pop();
            var cell = Board.GetCell(cmd.Row, cmd.Col);

            cell.Value = cmd.NewValue;
            System.Array.Copy(cmd.NewNotes, cell.Notes, 9);

            _undoStack.Push(cmd);
            UpdateErrors();

            EventSystem.Instance.Publish(SudokuEvents.REDO_PERFORMED, this);
            EventSystem.Instance.Publish(SudokuEvents.CELL_VALUE_CHANGED, this);

            CheckCompletion();
        }

        #endregion

        #region 冲突检测

        /// <summary>
        /// 更新所有格子的冲突状态
        /// </summary>
        private void UpdateErrors()
        {
            // 先清除所有错误标记
            for (int i = 0; i < Board.Cells.Length; i++)
            {
                Board.Cells[i].IsError = false;
            }

            // 检查每行
            for (int r = 0; r < SudokuBoardData.SIZE; r++)
            {
                CheckGroupErrors(GetRowCells(r));
            }

            // 检查每列
            for (int c = 0; c < SudokuBoardData.SIZE; c++)
            {
                CheckGroupErrors(GetColCells(c));
            }

            // 检查每宫
            for (int br = 0; br < SudokuBoardData.BOX_SIZE; br++)
            {
                for (int bc = 0; bc < SudokuBoardData.BOX_SIZE; bc++)
                {
                    CheckGroupErrors(GetBoxCells(br, bc));
                }
            }

            EventSystem.Instance.Publish(SudokuEvents.CELL_ERROR_CHANGED, this);
        }

        /// <summary>
        /// 检查一组格子中是否有重复数字
        /// </summary>
        private void CheckGroupErrors(List<SudokuCellData> group)
        {
            for (int i = 0; i < group.Count; i++)
            {
                if (group[i].Value == 0) continue;
                for (int j = i + 1; j < group.Count; j++)
                {
                    if (group[j].Value == 0) continue;
                    if (group[i].Value == group[j].Value)
                    {
                        group[i].IsError = true;
                        group[j].IsError = true;
                    }
                }
            }
        }

        private List<SudokuCellData> GetRowCells(int row)
        {
            var cells = new List<SudokuCellData>(SudokuBoardData.SIZE);
            for (int c = 0; c < SudokuBoardData.SIZE; c++)
                cells.Add(Board.GetCell(row, c));
            return cells;
        }

        private List<SudokuCellData> GetColCells(int col)
        {
            var cells = new List<SudokuCellData>(SudokuBoardData.SIZE);
            for (int r = 0; r < SudokuBoardData.SIZE; r++)
                cells.Add(Board.GetCell(r, col));
            return cells;
        }

        private List<SudokuCellData> GetBoxCells(int boxRow, int boxCol)
        {
            var cells = new List<SudokuCellData>(SudokuBoardData.SIZE);
            int startR = boxRow * SudokuBoardData.BOX_SIZE;
            int startC = boxCol * SudokuBoardData.BOX_SIZE;
            for (int r = startR; r < startR + SudokuBoardData.BOX_SIZE; r++)
                for (int c = startC; c < startC + SudokuBoardData.BOX_SIZE; c++)
                    cells.Add(Board.GetCell(r, c));
            return cells;
        }

        #endregion

        #region 完成判定

        /// <summary>
        /// 检查游戏是否完成
        /// </summary>
        private void CheckCompletion()
        {
            if (Board.IsComplete)
            {
                CurrentState = GameState.Completed;
                GameLogger.LogInfo(TAG, $"Game completed! Time: {Board.ElapsedTime:F1}s");
                EventSystem.Instance.Publish(SudokuEvents.GAME_COMPLETED, this);
            }
        }

        #endregion

        /// <summary>
        /// 更新计时（由外部每帧调用）
        /// </summary>
        public void UpdateTimer(float deltaTime)
        {
            if (CurrentState != GameState.Playing) return;
            Board.ElapsedTime += deltaTime;
        }

        public override void Dispose()
        {
            Board = null;
            _undoStack.Clear();
            _redoStack.Clear();
            base.Dispose();
        }
    }
}
