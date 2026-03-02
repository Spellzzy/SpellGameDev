using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Log;

namespace GameFramework.Debug
{
    /// <summary>
    /// 运行时调试控制台。
    /// 支持注册自定义命令、查看日志、显示FPS。
    /// 仅在Development Build或Editor下可用。
    /// 按 BackQuote(`) 键切换显示。
    /// </summary>
    public class DebugConsole : MonoSingleton<DebugConsole>
    {
        private const string TAG = "DebugConsole";

        /// <summary>
        /// 调试命令委托
        /// </summary>
        public delegate void DebugCommand(string[] args);

        // 注册的命令
        private readonly Dictionary<string, DebugCommand> _commands = new Dictionary<string, DebugCommand>();
        private readonly Dictionary<string, string> _commandHelps = new Dictionary<string, string>();

        // 日志记录
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();
        private const int MAX_LOG_ENTRIES = 200;

        // UI状态
        private bool _isVisible = false;
        private string _inputText = "";
        private Vector2 _scrollPosition;
        private float _fps;
        private float _fpsTimer;
        private int _frameCount;

        // UI布局
        private Rect _windowRect;
        private const float WINDOW_WIDTH_RATIO = 0.6f;
        private const float WINDOW_HEIGHT_RATIO = 0.5f;

        private struct LogEntry
        {
            public string Message;
            public LogType Type;
            public string Timestamp;
        }

        protected override void OnInit()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            enabled = false;
            return;
#endif
            Application.logMessageReceived += OnLogReceived;
            RegisterBuiltInCommands();
            GameLogger.LogInfo(TAG, "DebugConsole initialized. Press ` to toggle.");
        }

        private void RegisterBuiltInCommands()
        {
            RegisterCommand("help", "显示所有命令", (args) =>
            {
                foreach (var kvp in _commandHelps)
                {
                    AddLog($"  {kvp.Key} - {kvp.Value}", LogType.Log);
                }
            });

            RegisterCommand("clear", "清空控制台", (args) =>
            {
                _logEntries.Clear();
            });

            RegisterCommand("fps", "显示/隐藏FPS", (args) =>
            {
                AddLog($"FPS: {_fps:F1}", LogType.Log);
            });

            RegisterCommand("timescale", "设置时间缩放 (用法: timescale 0.5)", (args) =>
            {
                if (args.Length > 0 && float.TryParse(args[0], out float scale))
                {
                    Time.timeScale = scale;
                    AddLog($"TimeScale set to {scale}", LogType.Log);
                }
                else
                {
                    AddLog($"Current TimeScale: {Time.timeScale}", LogType.Log);
                }
            });

            RegisterCommand("gc", "强制GC回收", (args) =>
            {
                GC.Collect();
                AddLog("GC.Collect() executed.", LogType.Log);
            });

            RegisterCommand("sysinfo", "显示系统信息", (args) =>
            {
                AddLog($"Device: {SystemInfo.deviceModel}", LogType.Log);
                AddLog($"OS: {SystemInfo.operatingSystem}", LogType.Log);
                AddLog($"GPU: {SystemInfo.graphicsDeviceName}", LogType.Log);
                AddLog($"RAM: {SystemInfo.systemMemorySize}MB", LogType.Log);
                AddLog($"VRAM: {SystemInfo.graphicsMemorySize}MB", LogType.Log);
                AddLog($"Screen: {Screen.width}x{Screen.height} @{Screen.currentResolution.refreshRate}Hz", LogType.Log);
            });

            RegisterCommand("quality", "设置画质等级 (用法: quality 0~5)", (args) =>
            {
                if (args.Length > 0 && int.TryParse(args[0], out int level))
                {
                    QualitySettings.SetQualityLevel(level);
                    AddLog($"Quality set to {QualitySettings.names[level]}", LogType.Log);
                }
                else
                {
                    AddLog($"Current Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}", LogType.Log);
                }
            });
        }

        /// <summary>
        /// 注册自定义调试命令
        /// </summary>
        /// <param name="name">命令名称</param>
        /// <param name="help">帮助文本</param>
        /// <param name="command">命令回调</param>
        public void RegisterCommand(string name, string help, DebugCommand command)
        {
            _commands[name.ToLower()] = command;
            _commandHelps[name.ToLower()] = help;
        }

        private void Update()
        {
            // FPS计算
            _frameCount++;
            _fpsTimer += Time.unscaledDeltaTime;
            if (_fpsTimer >= 1f)
            {
                _fps = _frameCount / _fpsTimer;
                _frameCount = 0;
                _fpsTimer = 0f;
            }

            // 切换显示
            if (UnityEngine.Input.GetKeyDown(KeyCode.BackQuote))
            {
                _isVisible = !_isVisible;
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            float w = Screen.width * WINDOW_WIDTH_RATIO;
            float h = Screen.height * WINDOW_HEIGHT_RATIO;
            _windowRect = new Rect((Screen.width - w) * 0.5f, 10, w, h);
            _windowRect = GUI.Window(9999, _windowRect, DrawWindow, $"Debug Console | FPS: {_fps:F1}");
        }

        private void DrawWindow(int windowId)
        {
            // 日志区域
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
            foreach (var entry in _logEntries)
            {
                Color color = entry.Type switch
                {
                    LogType.Error or LogType.Exception => Color.red,
                    LogType.Warning => Color.yellow,
                    _ => Color.white
                };
                GUI.contentColor = color;
                GUILayout.Label($"[{entry.Timestamp}] {entry.Message}");
            }
            GUI.contentColor = Color.white;
            GUILayout.EndScrollView();

            // 输入区域
            GUILayout.BeginHorizontal();
            _inputText = GUILayout.TextField(_inputText, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Execute", GUILayout.Width(80)) ||
                (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                ExecuteCommand(_inputText);
                _inputText = "";
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        private void ExecuteCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            AddLog($"> {input}", LogType.Log);

            var parts = input.Trim().Split(' ');
            string cmdName = parts[0].ToLower();
            string[] args = parts.Length > 1 ? input.Substring(parts[0].Length).Trim().Split(' ') : Array.Empty<string>();

            if (_commands.TryGetValue(cmdName, out var command))
            {
                try
                {
                    command.Invoke(args);
                }
                catch (Exception e)
                {
                    AddLog($"Command error: {e.Message}", LogType.Error);
                }
            }
            else
            {
                AddLog($"Unknown command: '{cmdName}'. Type 'help' for list.", LogType.Warning);
            }
        }

        private void OnLogReceived(string condition, string stackTrace, LogType type)
        {
            AddLog(condition, type);
        }

        private void AddLog(string message, LogType type)
        {
            _logEntries.Add(new LogEntry
            {
                Message = message,
                Type = type,
                Timestamp = DateTime.Now.ToString("HH:mm:ss")
            });

            if (_logEntries.Count > MAX_LOG_ENTRIES)
            {
                _logEntries.RemoveAt(0);
            }

            _scrollPosition = new Vector2(0, float.MaxValue);
        }

        protected override void OnDestroy()
        {
            Application.logMessageReceived -= OnLogReceived;
            base.OnDestroy();
        }
    }
}
