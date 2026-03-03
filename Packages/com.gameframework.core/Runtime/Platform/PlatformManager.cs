using GameFramework.Core;
using GameFramework.Log;

namespace GameFramework.Platform
{
    /// <summary>
    /// 平台管理器。
    /// 统一管理跨平台实现的注册和获取。
    /// 在 GameManager 初始化序列中最早初始化，其他 Manager 通过此类获取平台实现。
    /// 
    /// 使用方式：
    /// 1. 自动检测：默认根据编译符号自动选择平台实现
    /// 2. 手动指定：调用 Initialize(PlatformType) 强制切换
    /// 3. 自定义实现：调用 SetXxx() 方法注入自定义实现
    /// </summary>
    public class PlatformManager : Singleton<PlatformManager>
    {
        private const string TAG = "PlatformManager";

        private IPlatformStorage _storage;
        private IPlatformFileSystem _fileSystem;
        private IPlatformAudio _audio;
        private PlatformType _currentPlatform;

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

        protected override void OnInit()
        {
            // 根据条件编译符号自动检测平台
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

            switch (type)
            {
                case PlatformType.WXMiniGame:
                    InitWXMiniGame();
                    break;
                default:
                    InitDefault();
                    break;
            }

            // 初始化音频
            _audio?.Initialize();

            GameLogger.LogInfo(TAG, $"Platform initialized: {type}");
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
        /// 初始化微信小游戏平台实现
        /// </summary>
        private void InitWXMiniGame()
        {
#if WEIXINMINIGAME
            _storage = new WXStorage();
            _fileSystem = new WXFileSystem();
            _audio = new WXAudio();
#else
            // SDK 未导入时回退到默认实现
            GameLogger.LogWarning(TAG, "WEIXINMINIGAME symbol not defined, falling back to Default platform.");
            InitDefault();
#endif
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
