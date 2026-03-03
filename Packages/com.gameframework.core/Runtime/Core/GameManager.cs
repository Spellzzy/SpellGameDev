using UnityEngine;
using GameFramework.Event;
using GameFramework.FSM;
using GameFramework.Resource;
using GameFramework.Config;
using GameFramework.Save;
using GameFramework.Log;
using GameFramework.Platform;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏主管理器，框架入口。
    /// 负责初始化各子系统、驱动全局Update、管理应用生命周期。
    /// 挂载到场景中的启动物体上即可。
    /// </summary>
    public class GameManager : MonoSingleton<GameManager>
    {
        private const string TAG = "GameManager";

        /// <summary>
        /// 框架是否已完成初始化
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 游戏启动后经过的总时间（不受TimeScale影响）
        /// </summary>
        public float RealTimeSinceStartup => Time.realtimeSinceStartup;

        protected override void OnInit()
        {
            GameLogger.LogInfo(TAG, "GameManager initializing...");
            InitFramework();
        }

        /// <summary>
        /// 初始化框架各子系统（按依赖顺序）
        /// </summary>
        private void InitFramework()
        {
            // ── P0: 核心基础 ──
            _ = EventSystem.Instance;
            GameLogger.LogInfo(TAG, "EventSystem ready.");

            // ── P0.5: 平台抽象层（必须在 SaveManager/AudioManager 之前初始化）──
            _ = PlatformManager.Instance;
            GameLogger.LogInfo(TAG, $"PlatformManager ready. Platform: {PlatformManager.Instance.CurrentPlatform}");

            // ── P1: 流程骨架 ──
            _ = GameStateManager.Instance;
            GameLogger.LogInfo(TAG, "GameStateManager ready.");

            // ── P2: 核心运行时 ──
            _ = ResourceManager.Instance;
            GameLogger.LogInfo(TAG, "ResourceManager ready.");
            // UIManager & PoolManager 为 MonoSingleton，首次访问 Instance 时自动创建

            // ── P3: 内容支撑 ──
            _ = ConfigManager.Instance;
            GameLogger.LogInfo(TAG, "ConfigManager ready.");

            _ = SaveManager.Instance;
            GameLogger.LogInfo(TAG, "SaveManager ready.");
            // AudioManager 为 MonoSingleton，首次访问时自动创建

            // ── P4: 体验打磨 ──
            // InputManager, DebugConsole, TimerManager 均为 MonoSingleton，按需自动创建

            IsInitialized = true;
            GameLogger.LogInfo(TAG, "Framework initialized successfully.");
            EventSystem.Instance.Publish(GameEvents.APP_INIT_COMPLETE, this);
        }

        private void Update()
        {
            if (!IsInitialized) return;

            // 驱动游戏状态机
            GameStateManager.Instance.Update();

            // 驱动定时器（TimerManager自己在MonoBehaviour.Update中驱动，无需手动调用）
        }

        private void FixedUpdate()
        {
            if (!IsInitialized) return;

            GameStateManager.Instance.FixedUpdate();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                GameLogger.LogInfo(TAG, "Application paused.");
                EventSystem.Instance.Publish(GameEvents.APP_PAUSE, this);
            }
            else
            {
                GameLogger.LogInfo(TAG, "Application resumed.");
                EventSystem.Instance.Publish(GameEvents.APP_RESUME, this);
            }
        }

        private void OnApplicationQuit()
        {
            GameLogger.LogInfo(TAG, "Application quitting...");
            EventSystem.Instance.Publish(GameEvents.APP_QUIT, this);

            // 按依赖逆序清理
            SaveManager.Instance.Dispose();
            ConfigManager.Instance.Dispose();
            ResourceManager.Instance.Dispose();
            GameStateManager.Instance.Dispose();
            PlatformManager.Instance.Dispose();
            EventSystem.Instance.Dispose();
            IsInitialized = false;
        }
    }
}
