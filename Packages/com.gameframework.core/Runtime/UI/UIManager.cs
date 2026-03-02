using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Event;
using GameFramework.Log;

namespace GameFramework.UI
{
    /// <summary>
    /// UI管理器。
    /// 管理所有UI面板的打开/关闭/层级排序。
    /// 支持面板栈（用于返回上一面板）。
    /// </summary>
    public class UIManager : MonoSingleton<UIManager>
    {
        private const string TAG = "UIManager";

        /// <summary>
        /// UI面板信息配置
        /// </summary>
        [Serializable]
        public class PanelInfo
        {
            public string PanelName;
            public string PrefabPath; // Resources 下的路径
        }

        [Header("UI Root Canvas (场景中拖入)")]
        [SerializeField] private Canvas _uiCanvas;

        // 各层级根节点
        private readonly Dictionary<UILayer, Transform> _layerRoots = new Dictionary<UILayer, Transform>();

        // 已打开的面板
        private readonly Dictionary<string, UIPanel> _openPanels = new Dictionary<string, UIPanel>();

        // 面板栈（用于Back返回）
        private readonly Stack<UIPanel> _panelStack = new Stack<UIPanel>();

        // 面板路径注册表
        private readonly Dictionary<string, string> _panelPaths = new Dictionary<string, string>();

        protected override void OnInit()
        {
            if (_uiCanvas == null)
            {
                // 自动创建UI Canvas
                var canvasGo = new GameObject("[UICanvas]");
                canvasGo.transform.SetParent(transform);
                _uiCanvas = canvasGo.AddComponent<Canvas>();
                _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // 创建各层级根节点
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var layerGo = new GameObject($"Layer_{layer}");
                var rt = layerGo.AddComponent<RectTransform>();
                rt.SetParent(_uiCanvas.transform, false);
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                var canvas = layerGo.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = (int)layer;

                _layerRoots[layer] = rt;
            }

            GameLogger.LogInfo(TAG, "UIManager initialized.");
        }

        /// <summary>
        /// 注册面板预制体路径
        /// </summary>
        /// <param name="panelName">面板名称</param>
        /// <param name="prefabPath">Resources下的预制体路径</param>
        public void RegisterPanel(string panelName, string prefabPath)
        {
            _panelPaths[panelName] = prefabPath;
        }

        /// <summary>
        /// 批量注册面板路径
        /// </summary>
        public void RegisterPanels(PanelInfo[] panels)
        {
            foreach (var info in panels)
            {
                _panelPaths[info.PanelName] = info.PrefabPath;
            }
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <param name="args">传递给面板的参数</param>
        /// <param name="pushToStack">是否压入面板栈</param>
        /// <returns>面板实例</returns>
        public T OpenPanel<T>(object args = null, bool pushToStack = true) where T : UIPanel
        {
            string panelName = typeof(T).Name;
            return OpenPanel(panelName, args, pushToStack) as T;
        }

        /// <summary>
        /// 按名称打开面板
        /// </summary>
        public UIPanel OpenPanel(string panelName, object args = null, bool pushToStack = true)
        {
            // 已打开，直接刷新
            if (_openPanels.TryGetValue(panelName, out var existingPanel))
            {
                existingPanel.OnRefresh(args);
                return existingPanel;
            }

            // 加载预制体
            if (!_panelPaths.TryGetValue(panelName, out var prefabPath))
            {
                GameLogger.LogError(TAG, $"Panel '{panelName}' not registered. Call RegisterPanel first.");
                return null;
            }

            var prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                GameLogger.LogError(TAG, $"Failed to load panel prefab at '{prefabPath}'.");
                return null;
            }

            // 实例化
            var panel = prefab.GetComponent<UIPanel>();
            if (panel == null)
            {
                GameLogger.LogError(TAG, $"Prefab '{prefabPath}' does not have UIPanel component.");
                return null;
            }

            // 获取层级根节点
            if (!_layerRoots.TryGetValue(panel.Layer, out var layerRoot))
            {
                layerRoot = _layerRoots[UILayer.Normal];
            }

            var go = Instantiate(prefab, layerRoot);
            var uiPanel = go.GetComponent<UIPanel>();

            uiPanel.OnInit(args);
            uiPanel.OnOpen(args);

            _openPanels[panelName] = uiPanel;

            if (pushToStack)
            {
                // 隐藏栈顶面板
                if (_panelStack.Count > 0)
                {
                    var topPanel = _panelStack.Peek();
                    if (topPanel.HideOnOtherOpen)
                    {
                        topPanel.OnHide();
                    }
                }
                _panelStack.Push(uiPanel);
            }

            GameLogger.LogInfo(TAG, $"Panel '{panelName}' opened.");
            EventSystem.Instance.Publish(GameEvents.UI_PANEL_OPENED, this);
            return uiPanel;
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void ClosePanel<T>() where T : UIPanel
        {
            ClosePanel(typeof(T).Name);
        }

        /// <summary>
        /// 按名称关闭面板
        /// </summary>
        public void ClosePanel(string panelName)
        {
            if (!_openPanels.TryGetValue(panelName, out var panel))
            {
                GameLogger.LogWarning(TAG, $"Panel '{panelName}' is not open.");
                return;
            }

            panel.OnClose();
            panel.OnDispose();
            _openPanels.Remove(panelName);
            Destroy(panel.gameObject);

            GameLogger.LogInfo(TAG, $"Panel '{panelName}' closed.");
            EventSystem.Instance.Publish(GameEvents.UI_PANEL_CLOSED, this);
        }

        /// <summary>
        /// 返回上一面板（弹出栈顶）
        /// </summary>
        public void Back()
        {
            if (_panelStack.Count <= 1)
            {
                GameLogger.LogDebug(TAG, "No panel to go back to.");
                return;
            }

            var current = _panelStack.Pop();
            ClosePanel(current.PanelName);

            // 恢复栈顶面板
            if (_panelStack.Count > 0)
            {
                var topPanel = _panelStack.Peek();
                topPanel.OnResume();
            }
        }

        /// <summary>
        /// 获取已打开的面板
        /// </summary>
        public T GetPanel<T>() where T : UIPanel
        {
            if (_openPanels.TryGetValue(typeof(T).Name, out var panel))
            {
                return panel as T;
            }
            return null;
        }

        /// <summary>
        /// 面板是否已打开
        /// </summary>
        public bool IsPanelOpen<T>() where T : UIPanel
        {
            return _openPanels.ContainsKey(typeof(T).Name);
        }

        /// <summary>
        /// 关闭所有面板
        /// </summary>
        public void CloseAll()
        {
            var panelNames = new List<string>(_openPanels.Keys);
            foreach (var name in panelNames)
            {
                ClosePanel(name);
            }
            _panelStack.Clear();
        }
    }
}
