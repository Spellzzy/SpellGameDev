using System;
using GameFramework.Core;
using GameFramework.Log;

namespace GameFramework.Platform
{
    /// <summary>
    /// 平台工厂委托。
    /// 由各平台程序集注册，用于创建平台实现而不产生直接引用依赖。
    /// </summary>
    public delegate void PlatformFactoryDelegate(PlatformManager manager);

    /// <summary>
    /// 平台管理器。
    /// 统一管理跨平台实现的注册和获取。
    /// 在 GameManager 初始化序列中最早初始化，其他 Manager 通过此类获取平台实现。
    /// 
    /// 使用方式：
    /// 1. 自动检测：默认根据编译符号自动选择平台实现
    /// 2. 注册工厂：各平台程序集通过 RegisterFactory 注册创建逻辑
    /// 3. 手动指定：调用 SetXxx() 方法注入自定义实现
    /// 
    /// 架构说明：
    /// WX 等平台实现位于独立程序集（带 defineConstraints），
    /// 通过 [RuntimeInitializeOnLoadMethod] 自动注册工厂到 PlatformManager，
    /// 避免 GameFramework.Runtime 直接引用平台 SDK 程序集。
    /// </summary>
    public class PlatformManager : Singleton<PlatformManager>
    {
        private const string TAG = "PlatformManager";

        private IPlatformStorage _storage;
        private IPlatformFileSystem _fileSystem;
        private IPlatformAudio _audio;
        private PlatformType _currentPlatform;

        private static PlatformFactoryDelegate _registeredFactory;

        /// <summary>
        /// 当前平台类型
        /// </summary>
        public PlatformType CurrentPlatform => _currentPlatform;

        /// <summary>
        /// 平台键值存储
        /// </summary>
        public IPlatformStorage Storage => _storage;

        /// <summary>
        /// 平台文件系统
        /// </summary>
        public IPlatformFileSystem FileSystem => _fileSystem;

        /// <summary>
        /// 平台音频
        /// </summary>
        public IPlatformAudio Audio => _audio;

        /// <summary>
        /// 是否为微信小游戏平台
        /// </summary>
        public bool IsWXMiniGame => _currentPlatform == PlatformType.WXMiniGame;

        /// <summary>
        /// 注册平台工厂。
        /// 各平台程序集在 [RuntimeInitializeOnLoadMethod] 中调用此方法。
        /// </summary>
        /// <param name="factory">平台创建委托</param>
        public static void RegisterFactory(PlatformFactoryDelegate factory)
        {
            _registeredFactory = factory;
        }

        /// <summary>
        /// 初始化时自动检测平台
        /// </summary>
        protected override void OnInit()
        {
            PlatformType detectedType = DetectPlatform();
            Initialize(detectedType);
        }

        /// <summary>
        /// 初始化指定平台的所有实现。
        /// 通常在 GameManager.InitFramework 中调用。
        /// </summary>
        /// <param name="type">目标平台类型</param>
        public void Initialize(PlatformType type)
        {
            _currentPlatform = type;

            if (type != PlatformType.Default && _registeredFactory != null)
            {
                _registeredFactory(this);
            }
            else
            {
                if (type != PlatformType.Default)
                {
                    GameLogger.LogWarning(TAG,
                        $"No factory registered for {type}, falling back to Default.");
                    _currentPlatform = PlatformType.Default;
                }
                InitDefault();
            }

            // 初始化音频
            _audio?.Initialize();

            GameLogger.LogInfo(TAG, $"Platform initialized: {_currentPlatform}");
        }

        /// <summary>
        /// 设置自定义键值存储实现
        /// </summary>
        public void SetStorage(IPlatformStorage storage)
        {
            _storage = storage;
            GameLogger.LogInfo(TAG, $"Storage switched to {storage.GetType().Name}");
        }

        /// <summary>
        /// 设置自定义文件系统实现
        /// </summary>
        public void SetFileSystem(IPlatformFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            GameLogger.LogInfo(TAG, $"FileSystem switched to {fileSystem.GetType().Name}");
        }

        /// <summary>
        /// 设置自定义音频实现
        /// </summary>
        public void SetAudio(IPlatformAudio audio)
        {
            // 释放旧实现
            _audio?.Dispose();
            _audio = audio;
            _audio?.Initialize();
            GameLogger.LogInfo(TAG, $"Audio switched to {audio.GetType().Name}");
        }

        /// <summary>
        /// 检测当前编译平台
        /// </summary>
        private PlatformType DetectPlatform()
        {
#if WEIXINMINIGAME
            return PlatformType.WXMiniGame;
#else
            return PlatformType.Default;
#endif
        }

        /// <summary>
        /// 初始化默认平台实现
        /// </summary>
        private void InitDefault()
        {
            _storage = new DefaultStorage();
            _fileSystem = new DefaultFileSystem();
            _audio = new DefaultAudio();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            _audio?.Dispose();
            _audio = null;
            _storage = null;
            _fileSystem = null;
            base.Dispose();
        }
    }
}
