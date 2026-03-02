using UnityEngine;
using UnityEngine.UI;
using GameFramework.Event;
using Sudoku.Data;
using Sudoku.Logic;

namespace Sudoku.UI
{
    /// <summary>
    /// 游戏完成结算面板。
    /// 显示用时、难度、是否为最佳成绩，提供返回主菜单/再来一局按钮。
    /// </summary>
    public class GameCompleteView : MonoBehaviour
    {
        [Header("显示组件")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _timeText;
        [SerializeField] private Text _difficultyText;
        [SerializeField] private Text _bestTimeText;

        [Header("按钮")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _menuButton;

        [Header("面板引用")]
        [SerializeField] private GameObject _gamePanel;
        [SerializeField] private MainMenuView _mainMenuView;

        private void Start()
        {
            _panel.SetActive(false);

            EventSystem.Instance.Subscribe(SudokuEvents.GAME_COMPLETED, OnGameCompleted);

            _newGameButton.onClick.AddListener(OnNewGame);
            _menuButton.onClick.AddListener(OnBackToMenu);
        }

        /// <summary>
        /// 游戏完成回调
        /// </summary>
        private void OnGameCompleted(object sender)
        {
            var manager = SudokuManager.Instance;
            var board = manager.Board;

            // 记录统计
            SudokuStatsManager.Instance.RecordGame(
                board.Difficulty, true, board.ElapsedTime);

            // 显示结算面板
            _panel.SetActive(true);

            if (_titleText != null)
                _titleText.text = "恭喜完成！";

            if (_timeText != null)
            {
                int min = (int)(board.ElapsedTime / 60f);
                int sec = (int)(board.ElapsedTime % 60f);
                _timeText.text = $"用时: {min:00}:{sec:00}";
            }

            if (_difficultyText != null)
            {
                _difficultyText.text = board.Difficulty switch
                {
                    Difficulty.Easy => "难度: 简单",
                    Difficulty.Medium => "难度: 中等",
                    Difficulty.Hard => "难度: 困难",
                    Difficulty.Expert => "难度: 专家",
                    _ => ""
                };
            }

            if (_bestTimeText != null)
            {
                float best = SudokuStatsManager.Instance.Stats.BestTime[(int)board.Difficulty];
                if (best > 0f && best <= board.ElapsedTime)
                {
                    int bMin = (int)(best / 60f);
                    int bSec = (int)(best % 60f);
                    _bestTimeText.text = $"最佳: {bMin:00}:{bSec:00}";
                }
                else
                {
                    _bestTimeText.text = "新纪录！";
                }
            }
        }

        /// <summary>
        /// 再来一局（同难度）
        /// </summary>
        private void OnNewGame()
        {
            _panel.SetActive(false);
            var difficulty = SudokuManager.Instance.Board.Difficulty;
            SudokuManager.Instance.StartNewGame(difficulty);
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        private void OnBackToMenu()
        {
            _panel.SetActive(false);
            SudokuManager.Instance.ReturnToMenu();

            if (_gamePanel != null) _gamePanel.SetActive(false);
            if (_mainMenuView != null) _mainMenuView.ShowMenu();
        }

        private void OnDestroy()
        {
            if (EventSystem.HasInstance)
                EventSystem.Instance.Unsubscribe(SudokuEvents.GAME_COMPLETED, OnGameCompleted);

            _newGameButton.onClick.RemoveAllListeners();
            _menuButton.onClick.RemoveAllListeners();
        }
    }
}
