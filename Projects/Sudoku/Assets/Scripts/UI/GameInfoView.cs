using UnityEngine;
using UnityEngine.UI;
using GameFramework.Event;
using Sudoku.Data;
using Sudoku.Logic;

namespace Sudoku.UI
{
    /// <summary>
    /// 游戏信息栏（难度、错误数、剩余提示数）。
    /// </summary>
    public class GameInfoView : MonoBehaviour
    {
        [SerializeField] private Text _difficultyText;
        [SerializeField] private Text _errorsText;
        [SerializeField] private Text _hintsText;

        private void OnEnable()
        {
            EventSystem.Instance.Subscribe(SudokuEvents.BOARD_CREATED, OnRefresh);
            EventSystem.Instance.Subscribe(SudokuEvents.CELL_VALUE_CHANGED, OnRefresh);
            EventSystem.Instance.Subscribe(SudokuEvents.HINT_USED, OnRefresh);

            // 面板激活时如果已有棋盘数据，立即刷新
            if (SudokuManager.Instance.Board != null)
            {
                OnRefresh(null);
            }
        }

        private void OnDisable()
        {
            if (!EventSystem.HasInstance) return;
            EventSystem.Instance.Unsubscribe(SudokuEvents.BOARD_CREATED, OnRefresh);
            EventSystem.Instance.Unsubscribe(SudokuEvents.CELL_VALUE_CHANGED, OnRefresh);
            EventSystem.Instance.Unsubscribe(SudokuEvents.HINT_USED, OnRefresh);
        }

        private void OnRefresh(object sender)
        {
            var board = SudokuManager.Instance.Board;
            if (board == null) return;

            if (_difficultyText != null)
            {
                _difficultyText.text = board.Difficulty switch
                {
                    Difficulty.Easy => "简单",
                    Difficulty.Medium => "中等",
                    Difficulty.Hard => "困难",
                    Difficulty.Expert => "专家",
                    _ => ""
                };
            }

            if (_errorsText != null)
                _errorsText.text = $"错误: {board.ErrorCount}";

            if (_hintsText != null)
                _hintsText.text = $"提示: {board.HintsRemaining}";
        }

        private void OnDestroy()
        {
            // OnDisable 已处理取消订阅
        }
    }
}
