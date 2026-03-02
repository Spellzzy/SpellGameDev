using System;

namespace Sudoku.Data
{
    /// <summary>
    /// 游戏统计数据。
    /// 记录各难度的游玩和完成情况。
    /// </summary>
    [Serializable]
    public class SudokuStatsData
    {
        /// <summary>
        /// 各难度已玩局数（索引对应 Difficulty 枚举）
        /// </summary>
        public int[] GamesPlayed;

        /// <summary>
        /// 各难度已赢局数
        /// </summary>
        public int[] GamesWon;

        /// <summary>
        /// 各难度最佳用时（秒，0表示无记录）
        /// </summary>
        public float[] BestTime;

        /// <summary>
        /// 各难度累计用时（秒）
        /// </summary>
        public float[] TotalTime;

        /// <summary>
        /// 总连胜次数
        /// </summary>
        public int CurrentWinStreak;

        /// <summary>
        /// 最长连胜
        /// </summary>
        public int BestWinStreak;

        private const int DIFFICULTY_COUNT = 4;

        public SudokuStatsData()
        {
            GamesPlayed = new int[DIFFICULTY_COUNT];
            GamesWon = new int[DIFFICULTY_COUNT];
            BestTime = new float[DIFFICULTY_COUNT];
            TotalTime = new float[DIFFICULTY_COUNT];
        }

        /// <summary>
        /// 获取指定难度的胜率
        /// </summary>
        public float GetWinRate(Difficulty difficulty)
        {
            int idx = (int)difficulty;
            if (GamesPlayed[idx] == 0) return 0f;
            return (float)GamesWon[idx] / GamesPlayed[idx];
        }

        /// <summary>
        /// 获取指定难度的平均用时
        /// </summary>
        public float GetAverageTime(Difficulty difficulty)
        {
            int idx = (int)difficulty;
            if (GamesWon[idx] == 0) return 0f;
            return TotalTime[idx] / GamesWon[idx];
        }
    }
}
