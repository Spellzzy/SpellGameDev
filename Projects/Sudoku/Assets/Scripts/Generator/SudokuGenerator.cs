using System;
using System.Collections.Generic;
using Sudoku.Data;

namespace Sudoku.Generator
{
    /// <summary>
    /// 数独题目生成器。
    /// 
    /// 算法流程：
    /// 1. 回溯法生成一个完整的合法 9x9 解
    /// 2. 按难度等级逐步挖空
    /// 3. 每次挖空后验证仍然是唯一解
    /// 
    /// 使用：var (puzzle, solution) = SudokuGenerator.Generate(Difficulty.Medium);
    /// </summary>
    public static class SudokuGenerator
    {
        private const int SIZE = 9;
        private const int BOX = 3;
        private static readonly Random _random = new Random();

        /// <summary>
        /// 生成数独题目
        /// </summary>
        /// <param name="difficulty">难度等级</param>
        /// <returns>题目数组和答案数组（一维，长度81）</returns>
        public static (int[] puzzle, int[] solution) Generate(Difficulty difficulty)
        {
            // Step 1: 生成完整解
            int[] solution = new int[SIZE * SIZE];
            GenerateFullBoard(solution);

            // Step 2: 复制一份作为题目，然后挖空
            int[] puzzle = new int[SIZE * SIZE];
            Array.Copy(solution, puzzle, solution.Length);

            int holesToDig = GetHoleCount(difficulty);
            DigHoles(puzzle, holesToDig);

            return (puzzle, solution);
        }

        /// <summary>
        /// 根据难度获取挖空数量
        /// </summary>
        private static int GetHoleCount(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => _random.Next(30, 36),
                Difficulty.Medium => _random.Next(36, 46),
                Difficulty.Hard => _random.Next(46, 53),
                Difficulty.Expert => _random.Next(53, 59),
                _ => 36
            };
        }

        #region 生成完整解（回溯法）

        /// <summary>
        /// 用回溯法生成完整的 9x9 数独解
        /// </summary>
        private static bool GenerateFullBoard(int[] board)
        {
            int emptyIdx = FindEmpty(board);
            if (emptyIdx == -1) return true; // 全部填满

            int row = emptyIdx / SIZE;
            int col = emptyIdx % SIZE;

            // 随机化数字顺序以产生不同的解
            var numbers = GetShuffledNumbers();

            foreach (int num in numbers)
            {
                if (IsValid(board, row, col, num))
                {
                    board[emptyIdx] = num;
                    if (GenerateFullBoard(board))
                    {
                        return true;
                    }
                    board[emptyIdx] = 0;
                }
            }

            return false;
        }

        /// <summary>
        /// 找到第一个空位
        /// </summary>
        private static int FindEmpty(int[] board)
        {
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == 0) return i;
            }
            return -1;
        }

        /// <summary>
        /// 获取随机排列的 1-9
        /// </summary>
        private static int[] GetShuffledNumbers()
        {
            int[] nums = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int i = nums.Length - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (nums[i], nums[j]) = (nums[j], nums[i]);
            }
            return nums;
        }

        #endregion

        #region 挖空

        /// <summary>
        /// 按指定数量挖空，确保唯一解
        /// </summary>
        private static void DigHoles(int[] puzzle, int count)
        {
            // 生成随机位置顺序
            var positions = new List<int>();
            for (int i = 0; i < SIZE * SIZE; i++)
            {
                positions.Add(i);
            }
            Shuffle(positions);

            int dug = 0;
            foreach (int pos in positions)
            {
                if (dug >= count) break;
                if (puzzle[pos] == 0) continue;

                int backup = puzzle[pos];
                puzzle[pos] = 0;

                // 验证唯一解
                if (CountSolutions(puzzle, 2) == 1)
                {
                    dug++;
                }
                else
                {
                    // 不是唯一解，恢复
                    puzzle[pos] = backup;
                }
            }
        }

        private static void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        #endregion

        #region 验证

        /// <summary>
        /// 检查在指定位置放指定数字是否合法
        /// </summary>
        public static bool IsValid(int[] board, int row, int col, int num)
        {
            // 检查行
            for (int c = 0; c < SIZE; c++)
            {
                if (board[row * SIZE + c] == num) return false;
            }

            // 检查列
            for (int r = 0; r < SIZE; r++)
            {
                if (board[r * SIZE + col] == num) return false;
            }

            // 检查 3x3 宫
            int boxRow = (row / BOX) * BOX;
            int boxCol = (col / BOX) * BOX;
            for (int r = boxRow; r < boxRow + BOX; r++)
            {
                for (int c = boxCol; c < boxCol + BOX; c++)
                {
                    if (board[r * SIZE + c] == num) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 计算解的数量（到达 limit 时提前终止）
        /// </summary>
        /// <param name="board">棋盘</param>
        /// <param name="limit">最多找几个解就停</param>
        /// <returns>解的数量（最多 limit）</returns>
        private static int CountSolutions(int[] board, int limit)
        {
            int count = 0;
            CountSolutionsRecursive(board, ref count, limit);
            return count;
        }

        private static bool CountSolutionsRecursive(int[] board, ref int count, int limit)
        {
            int emptyIdx = FindEmpty(board);
            if (emptyIdx == -1)
            {
                count++;
                return count >= limit;
            }

            int row = emptyIdx / SIZE;
            int col = emptyIdx % SIZE;

            for (int num = 1; num <= SIZE; num++)
            {
                if (IsValid(board, row, col, num))
                {
                    board[emptyIdx] = num;
                    if (CountSolutionsRecursive(board, ref count, limit))
                    {
                        board[emptyIdx] = 0;
                        return true;
                    }
                    board[emptyIdx] = 0;
                }
            }

            return false;
        }

        #endregion
    }
}
