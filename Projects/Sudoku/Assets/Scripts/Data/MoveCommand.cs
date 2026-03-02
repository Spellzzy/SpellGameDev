using System;

namespace Sudoku.Data
{
    /// <summary>
    /// 操作命令，用于撤销/重做系统。
    /// 记录一次格子操作前后的完整状态。
    /// </summary>
    [Serializable]
    public class MoveCommand
    {
        /// <summary>
        /// 操作的格子行
        /// </summary>
        public int Row;

        /// <summary>
        /// 操作的格子列
        /// </summary>
        public int Col;

        /// <summary>
        /// 操作前的数值
        /// </summary>
        public int OldValue;

        /// <summary>
        /// 操作后的数值
        /// </summary>
        public int NewValue;

        /// <summary>
        /// 操作前的笔记状态
        /// </summary>
        public bool[] OldNotes;

        /// <summary>
        /// 操作后的笔记状态
        /// </summary>
        public bool[] NewNotes;

        /// <summary>
        /// 是否是笔记模式操作
        /// </summary>
        public bool IsNoteMode;

        public MoveCommand(int row, int col, int oldValue, int newValue,
            bool[] oldNotes, bool[] newNotes, bool isNoteMode)
        {
            Row = row;
            Col = col;
            OldValue = oldValue;
            NewValue = newValue;
            OldNotes = oldNotes;
            NewNotes = newNotes;
            IsNoteMode = isNoteMode;
        }
    }
}
