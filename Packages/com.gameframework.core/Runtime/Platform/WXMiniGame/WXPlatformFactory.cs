// ============================================================================
// 微信小游戏平台工厂注册。
// 在 RuntimeInitializeOnLoadMethod 阶段自动注册到 PlatformManager，
// 避免 GameFramework.Runtime 直接引用 WeChatWASM 程序集。
// ============================================================================

#if WEIXINMINIGAME

using UnityEngine;

namespace GameFramework.Platform
{
    /// <summary>
    /// 微信小游戏平台工厂。
    /// 通过 [RuntimeInitializeOnLoadMethod] 在游戏启动时自动注册。
    /// </summary>
    public static class WXPlatformFactory
    {
        /// <summary>
        /// 自动注册微信平台工厂到 PlatformManager
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            PlatformManager.RegisterFactory(CreateWXPlatform);
        }

        /// <summary>
        /// 创建微信小游戏平台实现并注入到 PlatformManager
        /// </summary>
        private static void CreateWXPlatform(PlatformManager manager)
        {
            manager.SetStorage(new WXStorage());
            manager.SetFileSystem(new WXFileSystem());
            manager.SetAudio(new WXAudio());
        }
    }
}

#endif
