using UnityEngine;
using UnityEngine.UI;
using Sudoku.Data;
using Sudoku.Logic;

namespace Sudoku.UI
{
    /// <summary>
    /// 统计面板。
    /// 显示各难度的完成次数、胜率、最佳用时。
    /// </summary>
    public class StatsView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _backButton;
        [SerializeField] private MainMenuView _mainMenuView;

        [Header("统计文本")]
        [SerializeField] private Text _easyStatsText;
        [SerializeField] private Text _mediumStatsText;
        [SerializeField] private Text _hardStatsText;
        [SerializeField] private Text _expertStatsText;
        [SerializeField] private Text _streakText;

        private void Start()
        {
            _backButton.onClick.AddListener(OnBack);
        }

        private void OnEnable()
        {
            Refresh();
        }

        /// <summary>
        /// 刷新统计显示
        /// </summary>
        private void Refresh()
        {
            var stats = SudokuStatsManager.Instance.Stats;

            SetStatsText(_easyStatsText, stats, Difficulty.Easy, "简单");
            SetStatsText(_mediumStatsText, stats, Difficulty.Medium, "中等");
            SetStatsText(_hardStatsText, stats, Difficulty.Hard, "困难");
            SetStatsText(_expertStatsText, stats, Difficulty.Expert, "专家");

            if (_streakText != null)
            {
                _streakText.text = $"当前连胜: {stats.CurrentWinStreak}  最长连胜: {stats.BestWinStreak}";
            }
        }

        /// <summary>
        /// 设置某个难度的统计文本
        /// </summary>
        private void SetStatsText(Text text, SudokuStatsData stats, Difficulty diff, string label)
        {
            if (text == null) return;

            int idx = (int)diff;
            int played = stats.GamesPlayed[idx];
            int won = stats.GamesWon[idx];
            float winRate = stats.GetWinRate(diff) * 100f;

            string bestStr = "--:--";
            if (stats.BestTime[idx] > 0f)
            {
                int min = (int)(stats.BestTime[idx] / 60f);
                int sec = (int)(stats.BestTime[idx] % 60f);
                bestStr = $"{min:00}:{sec:00}";
            }

            text.text = $"{label}  |  已玩: {played}  胜: {won}  胜率: {winRate:F0}%  最佳: {bestStr}";
        }

        private void OnBack()
        {
            _panel.SetActive(false);
            if (_mainMenuView != null) _mainMenuView.ShowMenu();
        }

        private void OnDestroy()
        {
            _backButton.onClick.RemoveAllListeners();
        }
    }
}
