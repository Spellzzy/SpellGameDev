namespace Sudoku.Data
{
    /// <summary>
    /// 数独难度等级
    /// </summary>
    public enum Difficulty
    {
        Easy,       // 挖空 30~35
        Medium,     // 挖空 36~45
        Hard,       // 挖空 46~52
        Expert      // 挖空 53~58
    }

    /// <summary>
    /// 游戏状态
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        Completed
    }
}
