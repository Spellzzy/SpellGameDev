using System;
using GameFramework.Core;
using GameFramework.Log;

namespace GameFramework.Resource
{
    /// <summary>
    /// 资源管理器。
    /// 提供统一的资源加载/卸载入口，底层通过 IResourceLoader 接口实现可插拔。
    /// 默认使用 ResourcesLoader，可随时切换为 Addressables 等实现。
    /// </summary>
    public class ResourceManager : Singleton<ResourceManager>
    {
        private const string TAG = "ResourceManager";

        private IResourceLoader _loader;

        /// <summary>
        /// 当前使用的资源加载器
        /// </summary>
        public IResourceLoader Loader => _loader;

        protected override void OnInit()
        {
            // 默认使用 Resources 方式
            _loader = new ResourcesLoader();
            GameLogger.LogInfo(TAG, "ResourceManager initialized with ResourcesLoader.");
        }

        /// <summary>
        /// 切换资源加载器实现（如切换到Addressables）
        /// </summary>
        /// <param name="loader">新的加载器实现</param>
        public void SetLoader(IResourceLoader loader)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            GameLogger.LogInfo(TAG, $"ResourceLoader switched to {loader.GetType().Name}.");
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            return _loader.Load<T>(path);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public void LoadAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
        {
            _loader.LoadAsync(path, onComplete);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        public void Unload(UnityEngine.Object asset)
        {
            _loader.Unload(asset);
        }

        /// <summary>
        /// 卸载所有未使用资源
        /// </summary>
        public void UnloadUnused()
        {
            _loader.UnloadUnused();
        }

        public override void Dispose()
        {
            _loader = null;
            base.Dispose();
        }
    }
}
