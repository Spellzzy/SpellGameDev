using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Log;

namespace GameFramework.Pool
{
    /// <summary>
    /// 对象池管理器。
    /// 统一管理多个ObjectPool，按预制体名称索引。
    /// </summary>
    public class PoolManager : MonoSingleton<PoolManager>
    {
        private const string TAG = "PoolManager";

        private readonly Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();
        private Transform _poolRoot;

        protected override void OnInit()
        {
            _poolRoot = new GameObject("[PoolRoot]").transform;
            _poolRoot.SetParent(transform);
            GameLogger.LogInfo(TAG, "PoolManager initialized.");
        }

        /// <summary>
        /// 创建或获取指定预制体的对象池
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="initialSize">初始预热数量</param>
        /// <param name="maxSize">最大容量</param>
        /// <returns>对象池</returns>
        public ObjectPool GetOrCreatePool(GameObject prefab, int initialSize = 0, int maxSize = 0)
        {
            string key = prefab.name;
            if (_pools.TryGetValue(key, out var pool))
            {
                return pool;
            }

            var parent = new GameObject($"Pool_{key}").transform;
            parent.SetParent(_poolRoot);

            pool = new ObjectPool(prefab, parent, initialSize, maxSize);
            _pools[key] = pool;
            GameLogger.LogDebug(TAG, $"Pool '{key}' created (init={initialSize}, max={maxSize}).");
            return pool;
        }

        /// <summary>
        /// 从指定预制体池中获取对象
        /// </summary>
        public GameObject Get(GameObject prefab, Vector3 position = default, Quaternion rotation = default)
        {
            var pool = GetOrCreatePool(prefab);
            return pool.Get(position, rotation);
        }

        /// <summary>
        /// 归还对象到对应的池
        /// </summary>
        public void Return(GameObject obj)
        {
            if (obj == null) return;

            string key = obj.name;
            if (_pools.TryGetValue(key, out var pool))
            {
                pool.Return(obj);
            }
            else
            {
                GameLogger.LogWarning(TAG, $"No pool found for '{key}', destroying directly.");
                Destroy(obj);
            }
        }

        /// <summary>
        /// 清空指定池
        /// </summary>
        public void ClearPool(string prefabName)
        {
            if (_pools.TryGetValue(prefabName, out var pool))
            {
                pool.Clear();
                _pools.Remove(prefabName);
            }
        }

        /// <summary>
        /// 清空所有池
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
        }
    }
}
