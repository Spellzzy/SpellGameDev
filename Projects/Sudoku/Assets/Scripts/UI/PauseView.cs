using UnityEngine;
using UnityEngine.UI;
using Sudoku.Logic;

namespace Sudoku.UI
{
    /// <summary>
    /// 暂停面板。
    /// </summary>
    public class PauseView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _menuButton;

        [SerializeField] private GameObject _gamePanel;
        [SerializeField] private MainMenuView _mainMenuView;

        private void Start()
        {
            _panel.SetActive(false);

            _pauseButton.onClick.AddListener(OnPause);
            _resumeButton.onClick.AddListener(OnResume);
            _menuButton.onClick.AddListener(OnBackToMenu);
        }

        private void OnPause()
        {
            SudokuManager.Instance.Pause();
            _panel.SetActive(true);
        }

        private void OnResume()
        {
            SudokuManager.Instance.Resume();
            _panel.SetActive(false);
        }

        private void OnBackToMenu()
        {
            // 自动存档
            SaveCurrentGame();

            SudokuManager.Instance.ReturnToMenu();
            _panel.SetActive(false);
            if (_gamePanel != null) _gamePanel.SetActive(false);
            if (_mainMenuView != null) _mainMenuView.ShowMenu();
        }

        /// <summary>
        /// 存档当前游戏
        /// </summary>
        private void SaveCurrentGame()
        {
            var board = SudokuManager.Instance.Board;
            if (board != null)
            {
                GameFramework.Save.SaveManager.Instance.Save(board, 0, "sudoku_board");
            }
        }

        private void Update()
        {
            // ESC 键暂停/继续
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (SudokuManager.Instance.CurrentState == Data.GameState.Playing)
                    OnPause();
                else if (SudokuManager.Instance.CurrentState == Data.GameState.Paused)
                    OnResume();
            }
        }

        private void OnDestroy()
        {
            _pauseButton.onClick.RemoveAllListeners();
            _resumeButton.onClick.RemoveAllListeners();
            _menuButton.onClick.RemoveAllListeners();
        }
    }
}
