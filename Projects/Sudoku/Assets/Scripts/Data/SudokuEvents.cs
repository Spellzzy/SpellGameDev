namespace Sudoku.Data
{
    /// <summary>
    /// 数独游戏事件名称常量。
    /// </summary>
    public static class SudokuEvents
    {
        // ── 棋盘 ──
        public const string BOARD_CREATED = "Sudoku.BoardCreated";
        public const string CELL_VALUE_CHANGED = "Sudoku.CellValueChanged";
        public const string CELL_NOTES_CHANGED = "Sudoku.CellNotesChanged";
        public const string CELL_ERROR_CHANGED = "Sudoku.CellErrorChanged";
        public const string BOARD_COMPLETE = "Sudoku.BoardComplete";

        // ── 选择 ──
        public const string CELL_SELECTED = "Sudoku.CellSelected";
        public const string CELL_DESELECTED = "Sudoku.CellDeselected";

        // ── 游戏流程 ──
        public const string GAME_STARTED = "Sudoku.GameStarted";
        public const string GAME_PAUSED = "Sudoku.GamePaused";
        public const string GAME_RESUMED = "Sudoku.GameResumed";
        public const string GAME_COMPLETED = "Sudoku.GameCompleted";

        // ── 操作 ──
        public const string HINT_USED = "Sudoku.HintUsed";
        public const string UNDO_PERFORMED = "Sudoku.UndoPerformed";
        public const string REDO_PERFORMED = "Sudoku.RedoPerformed";
        public const string NOTE_MODE_TOGGLED = "Sudoku.NoteModeToggled";

        // ── 计时 ──
        public const string TIMER_UPDATED = "Sudoku.TimerUpdated";
    }
}
