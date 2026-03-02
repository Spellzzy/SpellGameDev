using System;

namespace Sudoku.Data
{
    /// <summary>
    /// 数独单格数据。
    /// 存储一个格子的所有状态信息。
    /// </summary>
    [Serializable]
    public class SudokuCellData
    {
        /// <summary>
        /// 行索引（0~8）
        /// </summary>
        public int Row;

        /// <summary>
        /// 列索引（0~8）
        /// </summary>
        public int Col;

        /// <summary>
        /// 当前填入的数字（0 = 空）
        /// </summary>
        public int Value;

        /// <summary>
        /// 是否为题目预置数字（不可修改）
        /// </summary>
        public bool IsFixed;

        /// <summary>
        /// 笔记候选数字标记（索引0~8 对应数字1~9）
        /// </summary>
        public bool[] Notes;

        /// <summary>
        /// 是否存在冲突
        /// </summary>
        public bool IsError;

        public SudokuCellData()
        {
            Notes = new bool[9];
        }

        public SudokuCellData(int row, int col) : this()
        {
            Row = row;
            Col = col;
        }

        /// <summary>
        /// 是否为空格
        /// </summary>
        public bool IsEmpty => Value == 0;

        /// <summary>
        /// 是否有任何笔记
        /// </summary>
        public bool HasNotes
        {
            get
            {
                for (int i = 0; i < 9; i++)
                {
                    if (Notes[i]) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 清除所有笔记
        /// </summary>
        public void ClearNotes()
        {
            for (int i = 0; i < 9; i++)
            {
                Notes[i] = false;
            }
        }

        /// <summary>
        /// 复制笔记数组
        /// </summary>
        public bool[] CloneNotes()
        {
            var copy = new bool[9];
            Array.Copy(Notes, copy, 9);
            return copy;
        }
    }
}
