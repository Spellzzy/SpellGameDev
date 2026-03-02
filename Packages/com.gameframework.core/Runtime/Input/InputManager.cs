using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Log;

namespace GameFramework.Input
{
    /// <summary>
    /// 输入管理器。
    /// 封装Unity输入系统，支持键盘/鼠标/触屏的统一抽象。
    /// 支持输入映射（Action Name -> Key）可在运行时重绑定。
    /// 后续可无缝接入 New Input System。
    /// </summary>
    public class InputManager : MonoSingleton<InputManager>
    {
        private const string TAG = "InputManager";

        /// <summary>
        /// 输入动作绑定
        /// </summary>
        [Serializable]
        public class InputBinding
        {
            public string ActionName;
            public KeyCode PrimaryKey;
            public KeyCode SecondaryKey;
        }

        /// <summary>
        /// 是否启用输入（用于暂停/对话时禁用）
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        // 动作绑定表
        private readonly Dictionary<string, InputBinding> _bindings = new Dictionary<string, InputBinding>();

        // 虚拟轴值（可被UI摇杆等外部设置）
        private readonly Dictionary<string, float> _virtualAxes = new Dictionary<string, float>();

        protected override void OnInit()
        {
            // 注册默认按键绑定
            RegisterDefaultBindings();
            GameLogger.LogInfo(TAG, "InputManager initialized.");
        }

        /// <summary>
        /// 注册默认按键绑定
        /// </summary>
        private void RegisterDefaultBindings()
        {
            Bind("MoveUp", KeyCode.W, KeyCode.UpArrow);
            Bind("MoveDown", KeyCode.S, KeyCode.DownArrow);
            Bind("MoveLeft", KeyCode.A, KeyCode.LeftArrow);
            Bind("MoveRight", KeyCode.D, KeyCode.RightArrow);
            Bind("Jump", KeyCode.Space, KeyCode.None);
            Bind("Attack", KeyCode.J, KeyCode.Mouse0);
            Bind("Interact", KeyCode.E, KeyCode.None);
            Bind("Pause", KeyCode.Escape, KeyCode.None);
            Bind("Inventory", KeyCode.Tab, KeyCode.I);
        }

        /// <summary>
        /// 绑定/重绑定按键
        /// </summary>
        public void Bind(string actionName, KeyCode primary, KeyCode secondary = KeyCode.None)
        {
            _bindings[actionName] = new InputBinding
            {
                ActionName = actionName,
                PrimaryKey = primary,
                SecondaryKey = secondary
            };
        }

        /// <summary>
        /// 获取按键绑定
        /// </summary>
        public InputBinding GetBinding(string actionName)
        {
            _bindings.TryGetValue(actionName, out var binding);
            return binding;
        }

        /// <summary>
        /// 按键是否按下（当帧）
        /// </summary>
        public bool GetActionDown(string actionName)
        {
            if (!IsEnabled) return false;
            if (!_bindings.TryGetValue(actionName, out var binding)) return false;

            return UnityEngine.Input.GetKeyDown(binding.PrimaryKey) ||
                   (binding.SecondaryKey != KeyCode.None && UnityEngine.Input.GetKeyDown(binding.SecondaryKey));
        }

        /// <summary>
        /// 按键是否持续按住
        /// </summary>
        public bool GetAction(string actionName)
        {
            if (!IsEnabled) return false;
            if (!_bindings.TryGetValue(actionName, out var binding)) return false;

            return UnityEngine.Input.GetKey(binding.PrimaryKey) ||
                   (binding.SecondaryKey != KeyCode.None && UnityEngine.Input.GetKey(binding.SecondaryKey));
        }

        /// <summary>
        /// 按键是否松开（当帧）
        /// </summary>
        public bool GetActionUp(string actionName)
        {
            if (!IsEnabled) return false;
            if (!_bindings.TryGetValue(actionName, out var binding)) return false;

            return UnityEngine.Input.GetKeyUp(binding.PrimaryKey) ||
                   (binding.SecondaryKey != KeyCode.None && UnityEngine.Input.GetKeyUp(binding.SecondaryKey));
        }

        /// <summary>
        /// 获取水平移动轴值（-1 ~ 1）
        /// </summary>
        public float GetHorizontal()
        {
            if (!IsEnabled) return 0f;

            // 优先使用虚拟轴（触屏摇杆等）
            if (_virtualAxes.TryGetValue("Horizontal", out float virtualVal) && Mathf.Abs(virtualVal) > 0.01f)
            {
                return virtualVal;
            }

            float value = 0f;
            if (GetAction("MoveRight")) value += 1f;
            if (GetAction("MoveLeft")) value -= 1f;
            return value;
        }

        /// <summary>
        /// 获取垂直移动轴值（-1 ~ 1）
        /// </summary>
        public float GetVertical()
        {
            if (!IsEnabled) return 0f;

            if (_virtualAxes.TryGetValue("Vertical", out float virtualVal) && Mathf.Abs(virtualVal) > 0.01f)
            {
                return virtualVal;
            }

            float value = 0f;
            if (GetAction("MoveUp")) value += 1f;
            if (GetAction("MoveDown")) value -= 1f;
            return value;
        }

        /// <summary>
        /// 获取移动方向向量
        /// </summary>
        public Vector2 GetMoveDirection()
        {
            var dir = new Vector2(GetHorizontal(), GetVertical());
            return dir.sqrMagnitude > 1f ? dir.normalized : dir;
        }

        /// <summary>
        /// 设置虚拟轴值（供UI摇杆等调用）
        /// </summary>
        public void SetVirtualAxis(string axisName, float value)
        {
            _virtualAxes[axisName] = value;
        }

        /// <summary>
        /// 清除虚拟轴
        /// </summary>
        public void ClearVirtualAxis(string axisName)
        {
            _virtualAxes.Remove(axisName);
        }

        /// <summary>
        /// 获取触摸数量
        /// </summary>
        public int TouchCount => UnityEngine.Input.touchCount;

        /// <summary>
        /// 获取触摸信息
        /// </summary>
        public Touch GetTouch(int index) => UnityEngine.Input.GetTouch(index);

        /// <summary>
        /// 鼠标位置
        /// </summary>
        public Vector3 MousePosition => UnityEngine.Input.mousePosition;
    }
}
