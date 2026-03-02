using System;

namespace Sudoku.Data
{
    /// <summary>
    /// 数独棋盘数据。
    /// 存储整局游戏的所有状态。
    /// </summary>
    [Serializable]
    public class SudokuBoardData
    {
        /// <summary>
        /// 9x9 格子数据（序列化用一维数组，访问用索引方法）
        /// </summary>
        public SudokuCellData[] Cells;

        /// <summary>
        /// 完整答案（用于提示和验证）
        /// </summary>
        public int[] Solution;

        /// <summary>
        /// 难度
        /// </summary>
        public Difficulty Difficulty;

        /// <summary>
        /// 已用时间（秒）
        /// </summary>
        public float ElapsedTime;

        /// <summary>
        /// 剩余提示次数
        /// </summary>
        public int HintsRemaining;

        /// <summary>
        /// 错误次数
        /// </summary>
        public int ErrorCount;

        public const int SIZE = 9;
        public const int BOX_SIZE = 3;
        public const int DEFAULT_HINTS = 3;

        public SudokuBoardData()
        {
            Cells = new SudokuCellData[SIZE * SIZE];
            Solution = new int[SIZE * SIZE];
            HintsRemaining = DEFAULT_HINTS;

            for (int r = 0; r < SIZE; r++)
            {
                for (int c = 0; c < SIZE; c++)
                {
                    Cells[r * SIZE + c] = new SudokuCellData(r, c);
                }
            }
        }

        /// <summary>
        /// 获取指定位置的格子
        /// </summary>
        public SudokuCellData GetCell(int row, int col)
        {
            return Cells[row * SIZE + col];
        }

        /// <summary>
        /// 获取指定位置的答案
        /// </summary>
        public int GetSolution(int row, int col)
        {
            return Solution[row * SIZE + col];
        }

        /// <summary>
        /// 设置答案
        /// </summary>
        public void SetSolution(int row, int col, int value)
        {
            Solution[row * SIZE + col] = value;
        }

        /// <summary>
        /// 棋盘是否全部填满
        /// </summary>
        public bool IsFull
        {
            get
            {
                for (int i = 0; i < Cells.Length; i++)
                {
                    if (Cells[i].Value == 0) return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 棋盘是否全部正确（无冲突且填满）
        /// </summary>
        public bool IsComplete
        {
            get
            {
                for (int i = 0; i < Cells.Length; i++)
                {
                    if (Cells[i].Value == 0 || Cells[i].IsError) return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 获取空格数量
        /// </summary>
        public int EmptyCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Cells.Length; i++)
                {
                    if (Cells[i].Value == 0) count++;
                }
                return count;
            }
        }
    }
}
