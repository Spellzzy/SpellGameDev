using UnityEngine;
using UnityEngine.UI;
using Sudoku.Logic;
using Sudoku.Data;

namespace Sudoku.UI
{
    /// <summary>
    /// 计时器显示。
    /// </summary>
    public class TimerView : MonoBehaviour
    {
        [SerializeField] private Text _timerText;

        private void Update()
        {
            var manager = SudokuManager.Instance;
            if (manager.Board == null || manager.CurrentState != GameState.Playing)
                return;

            // 驱动计时
            manager.UpdateTimer(Time.deltaTime);

            // 刷新显示
            float time = manager.Board.ElapsedTime;
            int minutes = (int)(time / 60f);
            int seconds = (int)(time % 60f);
            _timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
