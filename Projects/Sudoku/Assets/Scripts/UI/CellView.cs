using UnityEngine;
using UnityEngine.UI;
using Sudoku.Data;
using Sudoku.Logic;

namespace Sudoku.UI
{
    /// <summary>
    /// 单个数独格子的UI表现。
    /// 
    /// 职责：显示数字/笔记、响应点击、高亮/错误样式。
    /// 依赖：SudokuManager（读取数据）
    /// </summary>
    public class CellView : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private Text _valueText;
        [SerializeField] private Text[] _noteTexts; // 9个笔记文本（3x3布局）
        [SerializeField] private Image _background;
        [SerializeField] private Button _button;

        [Header("颜色配置")]
        [SerializeField] private Color _normalColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color _fixedColor = new Color(0.93f, 0.93f, 0.93f, 1f);
        [SerializeField] private Color _selectedColor = new Color(0.78f, 0.87f, 1f, 1f);
        [SerializeField] private Color _highlightColor = new Color(0.88f, 0.93f, 1f, 1f);
        [SerializeField] private Color _errorColor = new Color(1f, 0.85f, 0.85f, 1f);
        [SerializeField] private Color _fixedTextColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color _userTextColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        [SerializeField] private Color _errorTextColor = new Color(0.9f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color _noteTextColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        /// <summary>
        /// 格子坐标
        /// </summary>
        public int Row { get; private set; }
        public int Col { get; private set; }

        /// <summary>
        /// 初始化格子
        /// </summary>
        public void Init(int row, int col)
        {
            Row = row;
            Col = col;
            _button.onClick.AddListener(OnClick);
        }

        /// <summary>
        /// 根据数据刷新显示
        /// </summary>
        public void Refresh(SudokuCellData data, bool isSelected, bool isHighlighted)
        {
            // 显示数字或笔记
            if (data.Value != 0)
            {
                _valueText.text = data.Value.ToString();
                _valueText.gameObject.SetActive(true);
                SetNotesVisible(false);

                // 文字颜色
                if (data.IsError)
                    _valueText.color = _errorTextColor;
                else if (data.IsFixed)
                    _valueText.color = _fixedTextColor;
                else
                    _valueText.color = _userTextColor;
            }
            else
            {
                _valueText.gameObject.SetActive(false);
                RefreshNotes(data);
            }

            // 背景颜色
            if (isSelected)
                _background.color = _selectedColor;
            else if (data.IsError)
                _background.color = _errorColor;
            else if (isHighlighted)
                _background.color = _highlightColor;
            else if (data.IsFixed)
                _background.color = _fixedColor;
            else
                _background.color = _normalColor;
        }

        /// <summary>
        /// 刷新笔记显示
        /// </summary>
        private void RefreshNotes(SudokuCellData data)
        {
            if (_noteTexts == null || _noteTexts.Length < 9)
            {
                SetNotesVisible(false);
                return;
            }

            bool hasAnyNote = data.HasNotes;
            SetNotesVisible(hasAnyNote);

            if (!hasAnyNote) return;

            for (int i = 0; i < 9; i++)
            {
                if (_noteTexts[i] != null)
                {
                    _noteTexts[i].text = data.Notes[i] ? (i + 1).ToString() : "";
                    _noteTexts[i].color = _noteTextColor;
                }
            }
        }

        /// <summary>
        /// 显示/隐藏所有笔记文本
        /// </summary>
        private void SetNotesVisible(bool visible)
        {
            if (_noteTexts == null) return;
            for (int i = 0; i < _noteTexts.Length; i++)
            {
                if (_noteTexts[i] != null)
                    _noteTexts[i].gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// 点击回调
        /// </summary>
        private void OnClick()
        {
            SudokuManager.Instance.SelectCell(Row, Col);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}
