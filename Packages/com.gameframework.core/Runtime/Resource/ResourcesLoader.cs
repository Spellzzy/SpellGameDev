using System;
using System.Collections;
using UnityEngine;
using GameFramework.Log;

namespace GameFramework.Resource
{
    /// <summary>
    /// 基于Unity Resources文件夹的资源加载实现。
    /// 适合原型期和小项目。大项目建议切换到 Addressables 实现。
    /// </summary>
    public class ResourcesLoader : IResourceLoader
    {
        private const string TAG = "ResourcesLoader";

        public T Load<T>(string path) where T : UnityEngine.Object
        {
            var asset = Resources.Load<T>(path);
            if (asset == null)
            {
                GameLogger.LogError(TAG, $"Failed to load '{path}' as {typeof(T).Name}.");
            }
            return asset;
        }

        public void LoadAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
        {
            // 异步加载需要协程驱动，这里通过 ResourceManager 的 MonoBehaviour 来执行
            var request = Resources.LoadAsync<T>(path);
            if (request == null)
            {
                GameLogger.LogError(TAG, $"Failed to start async load for '{path}'.");
                onComplete?.Invoke(null);
                return;
            }

            // 使用回调方式，由 ResourceManager 驱动协程
            request.completed += (op) =>
            {
                var asset = request.asset as T;
                if (asset == null)
                {
                    GameLogger.LogError(TAG, $"Async load failed for '{path}' as {typeof(T).Name}.");
                }
                onComplete?.Invoke(asset);
            };
        }

        public void Unload(UnityEngine.Object asset)
        {
            if (asset != null)
            {
                Resources.UnloadAsset(asset);
            }
        }

        public void UnloadUnused()
        {
            Resources.UnloadUnusedAssets();
        }
    }
}
