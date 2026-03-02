using UnityEngine;
using UnityEngine.UI;
using Sudoku.Data;
using Sudoku.Logic;

namespace Sudoku.UI
{
    /// <summary>
    /// 主菜单面板。
    /// 选择难度 → 开始游戏。显示继续按钮（如果有存档）。
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [Header("按钮")]
        [SerializeField] private Button _easyButton;
        [SerializeField] private Button _mediumButton;
        [SerializeField] private Button _hardButton;
        [SerializeField] private Button _expertButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _statsButton;

        [Header("面板引用")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private GameObject _gamePanel;
        [SerializeField] private GameObject _statsPanel;

        private void Start()
        {
            _easyButton.onClick.AddListener(() => StartGame(Difficulty.Easy));
            _mediumButton.onClick.AddListener(() => StartGame(Difficulty.Medium));
            _hardButton.onClick.AddListener(() => StartGame(Difficulty.Hard));
            _expertButton.onClick.AddListener(() => StartGame(Difficulty.Expert));

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(OnContinue);
                // 如果有存档显示继续按钮
                bool hasSave = GameFramework.Save.SaveManager.Instance.HasSave(0, "sudoku_board");
                _continueButton.gameObject.SetActive(hasSave);
            }

            if (_statsButton != null)
                _statsButton.onClick.AddListener(OnShowStats);

            ShowMenu();
        }

        /// <summary>
        /// 显示主菜单
        /// </summary>
        public void ShowMenu()
        {
            _menuPanel.SetActive(true);
            if (_gamePanel != null) _gamePanel.SetActive(false);
            if (_statsPanel != null) _statsPanel.SetActive(false);
        }

        /// <summary>
        /// 开始新游戏
        /// </summary>
        private void StartGame(Difficulty difficulty)
        {
            SudokuManager.Instance.StartNewGame(difficulty);
            _menuPanel.SetActive(false);
            if (_gamePanel != null) _gamePanel.SetActive(true);
        }

        /// <summary>
        /// 继续游戏（读档）
        /// </summary>
        private void OnContinue()
        {
            var boardData = GameFramework.Save.SaveManager.Instance
                .Load<SudokuBoardData>(0, "sudoku_board");

            if (boardData != null)
            {
                // TODO: 实现从存档恢复棋盘
                GameFramework.Log.GameLogger.LogInfo("MainMenu", "Continue from save.");
            }

            _menuPanel.SetActive(false);
            if (_gamePanel != null) _gamePanel.SetActive(true);
        }

        /// <summary>
        /// 显示统计
        /// </summary>
        private void OnShowStats()
        {
            if (_statsPanel != null)
            {
                _menuPanel.SetActive(false);
                _statsPanel.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            _easyButton.onClick.RemoveAllListeners();
            _mediumButton.onClick.RemoveAllListeners();
            _hardButton.onClick.RemoveAllListeners();
            _expertButton.onClick.RemoveAllListeners();
            if (_continueButton != null) _continueButton.onClick.RemoveAllListeners();
            if (_statsButton != null) _statsButton.onClick.RemoveAllListeners();
        }
    }
}
