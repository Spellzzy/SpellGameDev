using UnityEngine;
using UnityEngine.UI;
using GameFramework.Event;
using Sudoku.Data;
using Sudoku.Logic;

namespace Sudoku.UI
{
    /// <summary>
    /// 数独棋盘UI表现层。
    /// 
    /// 职责：动态生成9x9格子、监听事件刷新显示。
    /// 依赖：SudokuManager（数据源）、EventSystem（事件监听）
    /// 使用：挂载到场景中的棋盘根物体上，设置 CellPrefab。
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Transform _boardRoot;
        [SerializeField] private GridLayoutGroup _gridLayout;

        [Header("分隔线")]
        [SerializeField] private Color _thinLineColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] private Color _thickLineColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        private CellView[,] _cells;
        private const int SIZE = SudokuBoardData.SIZE;

        private void OnEnable()
        {
            SubscribeEvents();

            // 面板激活时，如果已有棋盘数据（事件可能在面板隐藏时已发出），立即创建
            if (SudokuManager.Instance.Board != null)
            {
                CreateBoard();
                RefreshAll();
            }
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeEvents()
        {
            var es = EventSystem.Instance;
            es.Subscribe(SudokuEvents.BOARD_CREATED, OnBoardCreated);
            es.Subscribe(SudokuEvents.CELL_VALUE_CHANGED, OnCellChanged);
            es.Subscribe(SudokuEvents.CELL_NOTES_CHANGED, OnCellChanged);
            es.Subscribe(SudokuEvents.CELL_ERROR_CHANGED, OnCellChanged);
            es.Subscribe(SudokuEvents.CELL_SELECTED, OnCellChanged);
            es.Subscribe(SudokuEvents.CELL_DESELECTED, OnCellChanged);
            es.Subscribe(SudokuEvents.UNDO_PERFORMED, OnCellChanged);
            es.Subscribe(SudokuEvents.REDO_PERFORMED, OnCellChanged);
            es.Subscribe(SudokuEvents.HINT_USED, OnCellChanged);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        private void UnsubscribeEvents()
        {
            if (!GameFramework.Event.EventSystem.HasInstance) return;

            var es = EventSystem.Instance;
            es.Unsubscribe(SudokuEvents.BOARD_CREATED, OnBoardCreated);
            es.Unsubscribe(SudokuEvents.CELL_VALUE_CHANGED, OnCellChanged);
            es.Unsubscribe(SudokuEvents.CELL_NOTES_CHANGED, OnCellChanged);
            es.Unsubscribe(SudokuEvents.CELL_ERROR_CHANGED, OnCellChanged);
            es.Unsubscribe(SudokuEvents.CELL_SELECTED, OnCellChanged);
            es.Unsubscribe(SudokuEvents.CELL_DESELECTED, OnCellChanged);
            es.Unsubscribe(SudokuEvents.UNDO_PERFORMED, OnCellChanged);
            es.Unsubscribe(SudokuEvents.REDO_PERFORMED, OnCellChanged);
            es.Unsubscribe(SudokuEvents.HINT_USED, OnCellChanged);
        }

        /// <summary>
        /// 棋盘创建完成，生成格子UI
        /// </summary>
        private void OnBoardCreated(object sender)
        {
            CreateBoard();
            RefreshAll();
        }

        /// <summary>
        /// 格子数据变化，刷新显示
        /// </summary>
        private void OnCellChanged(object sender)
        {
            RefreshAll();
        }

        /// <summary>
        /// 生成 9x9 格子
        /// </summary>
        private void CreateBoard()
        {
            // 清除已有
            if (_cells != null)
            {
                foreach (var cell in _cells)
                {
                    if (cell != null)
                        Destroy(cell.gameObject);
                }
            }

            // 清空子物体
            for (int i = _boardRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_boardRoot.GetChild(i).gameObject);
            }

            _cells = new CellView[SIZE, SIZE];

            for (int r = 0; r < SIZE; r++)
            {
                for (int c = 0; c < SIZE; c++)
                {
                    var go = Instantiate(_cellPrefab, _boardRoot);
                    go.name = $"Cell_{r}_{c}";

                    var cellView = go.GetComponent<CellView>();
                    cellView.Init(r, c);
                    _cells[r, c] = cellView;
                }
            }
        }

        /// <summary>
        /// 刷新所有格子的显示
        /// </summary>
        private void RefreshAll()
        {
            if (_cells == null) return;

            var manager = SudokuManager.Instance;
            var board = manager.Board;
            if (board == null) return;

            var selected = manager.SelectedCell;

            for (int r = 0; r < SIZE; r++)
            {
                for (int c = 0; c < SIZE; c++)
                {
                    var data = board.GetCell(r, c);
                    bool isSelected = (r == selected.Row && c == selected.Col);
                    bool isHighlighted = IsHighlighted(r, c, selected, board);

                    _cells[r, c].Refresh(data, isSelected, isHighlighted);
                }
            }
        }

        /// <summary>
        /// 判断格子是否需要高亮（同行/同列/同宫/同数字）
        /// </summary>
        private bool IsHighlighted(int row, int col,
            (int Row, int Col) selected, SudokuBoardData board)
        {
            if (selected.Row < 0) return false;

            // 同行或同列
            if (row == selected.Row || col == selected.Col) return true;

            // 同宫
            int boxR1 = row / SudokuBoardData.BOX_SIZE;
            int boxC1 = col / SudokuBoardData.BOX_SIZE;
            int boxR2 = selected.Row / SudokuBoardData.BOX_SIZE;
            int boxC2 = selected.Col / SudokuBoardData.BOX_SIZE;
            if (boxR1 == boxR2 && boxC1 == boxC2) return true;

            // 同数字高亮
            var selectedCell = board.GetCell(selected.Row, selected.Col);
            if (selectedCell.Value != 0)
            {
                var thisCell = board.GetCell(row, col);
                if (thisCell.Value == selectedCell.Value) return true;
            }

            return false;
        }

        private void OnDestroy()
        {
            // OnDisable 已处理取消订阅
        }
    }
}
