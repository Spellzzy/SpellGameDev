using UnityEngine;
using UnityEngine.UI;
using Sudoku.Logic;

namespace Sudoku.UI
{
    /// <summary>
    /// 数字输入面板。
    /// 显示 1-9 数字按钮和功能按钮（擦除、笔记、提示、撤销、重做）。
    /// </summary>
    public class NumberPadView : MonoBehaviour
    {
        [Header("数字按钮（按顺序 1~9）")]
        [SerializeField] private Button[] _numberButtons;

        [Header("功能按钮")]
        [SerializeField] private Button _eraseButton;
        [SerializeField] private Button _noteButton;
        [SerializeField] private Button _hintButton;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _redoButton;

        [Header("笔记模式指示")]
        [SerializeField] private Image _noteIndicator;
        [SerializeField] private Color _noteActiveColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color _noteInactiveColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        private void Start()
        {
            // 绑定数字按钮
            for (int i = 0; i < _numberButtons.Length; i++)
            {
                int number = i + 1;
                _numberButtons[i].onClick.AddListener(() => OnNumberClick(number));
            }

            // 绑定功能按钮
            _eraseButton.onClick.AddListener(OnEraseClick);
            _noteButton.onClick.AddListener(OnNoteClick);
            _hintButton.onClick.AddListener(OnHintClick);
            _undoButton.onClick.AddListener(OnUndoClick);
            _redoButton.onClick.AddListener(OnRedoClick);
        }

        private void Update()
        {
            // 实时更新按钮状态
            var manager = SudokuManager.Instance;

            if (_undoButton != null)
                _undoButton.interactable = manager.CanUndo;
            if (_redoButton != null)
                _redoButton.interactable = manager.CanRedo;
            if (_hintButton != null && manager.Board != null)
                _hintButton.interactable = manager.Board.HintsRemaining > 0;
            if (_noteIndicator != null)
                _noteIndicator.color = manager.IsNoteMode ? _noteActiveColor : _noteInactiveColor;

            // 键盘快捷键
            HandleKeyboardInput();
        }

        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void HandleKeyboardInput()
        {
            var manager = SudokuManager.Instance;
            if (manager.CurrentState != Data.GameState.Playing) return;

            // 数字键 1-9
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    manager.SetNumber(i);
                    return;
                }
            }

            // Delete/Backspace 擦除
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                manager.ClearCell();
            }

            // N 切换笔记
            if (Input.GetKeyDown(KeyCode.N))
            {
                manager.ToggleNoteMode();
            }

            // Ctrl+Z 撤销
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand))
                && Input.GetKeyDown(KeyCode.Z))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    manager.Redo();
                else
                    manager.Undo();
            }

            // H 提示
            if (Input.GetKeyDown(KeyCode.H))
            {
                manager.UseHint();
            }
        }

        private void OnNumberClick(int number)
        {
            SudokuManager.Instance.SetNumber(number);
        }

        private void OnEraseClick()
        {
            SudokuManager.Instance.ClearCell();
        }

        private void OnNoteClick()
        {
            SudokuManager.Instance.ToggleNoteMode();
        }

        private void OnHintClick()
        {
            SudokuManager.Instance.UseHint();
        }

        private void OnUndoClick()
        {
            SudokuManager.Instance.Undo();
        }

        private void OnRedoClick()
        {
            SudokuManager.Instance.Redo();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _numberButtons.Length; i++)
                _numberButtons[i].onClick.RemoveAllListeners();

            _eraseButton.onClick.RemoveAllListeners();
            _noteButton.onClick.RemoveAllListeners();
            _hintButton.onClick.RemoveAllListeners();
            _undoButton.onClick.RemoveAllListeners();
            _redoButton.onClick.RemoveAllListeners();
        }
    }
}
