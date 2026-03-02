using UnityEngine;
using GameFramework.Core;
using GameFramework.Log;
using Sudoku.Logic;

namespace Sudoku.GameFlow
{
    /// <summary>
    /// 数独游戏入口。
    /// 挂载到场景中的启动物体上，初始化游戏框架和数独管理器。
    /// </summary>
    public class SudokuGameEntry : MonoBehaviour
    {
        private const string TAG = "SudokuEntry";

        private void Awake()
        {
            // 初始化框架
            _ = GameManager.Instance;

            // 初始化数独管理器
            _ = SudokuManager.Instance;
            _ = SudokuStatsManager.Instance;

            GameLogger.LogInfo(TAG, "Sudoku game initialized.");
        }
    }
}
