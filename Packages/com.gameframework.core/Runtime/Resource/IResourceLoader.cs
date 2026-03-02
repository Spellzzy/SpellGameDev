using System;
using UnityEngine;

namespace GameFramework.Resource
{
    /// <summary>
    /// 资源加载器接口。
    /// 通过接口抽象，可无缝切换 Resources / Addressables / AssetBundle 等实现。
    /// </summary>
    public interface IResourceLoader
    {
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <returns>资源实例</returns>
        T Load<T>(string path) where T : UnityEngine.Object;

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="onComplete">加载完成回调</param>
        void LoadAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object;

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="asset">资源实例</param>
        void Unload(UnityEngine.Object asset);

        /// <summary>
        /// 卸载所有未使用资源
        /// </summary>
        void UnloadUnused();
    }
}
