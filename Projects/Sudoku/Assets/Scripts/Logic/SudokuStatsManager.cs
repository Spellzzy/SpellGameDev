using GameFramework.Core;
using GameFramework.Log;
using GameFramework.Save;
using Sudoku.Data;

namespace Sudoku.Logic
{
    /// <summary>
    /// 数独统计管理器。
    /// 
    /// 职责：记录游戏统计数据、持久化存档。
    /// 依赖：SaveManager
    /// </summary>
    public class SudokuStatsManager : Singleton<SudokuStatsManager>
    {
        private const string TAG = "SudokuStats";
        private const string STATS_FILE = "sudoku_stats";

        /// <summary>
        /// 统计数据
        /// </summary>
        public SudokuStatsData Stats { get; private set; }

        protected override void OnInit()
        {
            // 尝试加载已有统计
            Stats = SaveManager.Instance.Load<SudokuStatsData>(0, STATS_FILE);
            if (Stats == null)
            {
                Stats = new SudokuStatsData();
            }
            GameLogger.LogInfo(TAG, "StatsManager initialized.");
        }

        /// <summary>
        /// 记录一局游戏结果
        /// </summary>
        /// <param name="difficulty">难度</param>
        /// <param name="won">是否胜利</param>
        /// <param name="time">用时（秒）</param>
        public void RecordGame(Difficulty difficulty, bool won, float time)
        {
            int idx = (int)difficulty;
            Stats.GamesPlayed[idx]++;

            if (won)
            {
                Stats.GamesWon[idx]++;
                Stats.TotalTime[idx] += time;

                // 更新最佳用时
                if (Stats.BestTime[idx] <= 0f || time < Stats.BestTime[idx])
                {
                    Stats.BestTime[idx] = time;
                }

                // 连胜
                Stats.CurrentWinStreak++;
                if (Stats.CurrentWinStreak > Stats.BestWinStreak)
                {
                    Stats.BestWinStreak = Stats.CurrentWinStreak;
                }
            }
            else
            {
                Stats.CurrentWinStreak = 0;
            }

            Save();
            GameLogger.LogInfo(TAG, $"Game recorded: {difficulty} {(won ? "WIN" : "LOSE")} {time:F1}s");
        }

        /// <summary>
        /// 保存统计数据
        /// </summary>
        public void Save()
        {
            SaveManager.Instance.Save(Stats, 0, STATS_FILE);
        }

        public override void Dispose()
        {
            Save();
            base.Dispose();
        }
    }
}
