using System.Collections.Generic;
using UnityEngine;
using GameFramework.Log;

namespace GameFramework.Pool
{
    /// <summary>
    /// 通用对象池，管理GameObject的复用。
    /// 减少频繁Instantiate/Destroy带来的GC和性能开销。
    /// </summary>
    public class ObjectPool
    {
        private const string TAG = "ObjectPool";

        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Queue<GameObject> _pool = new Queue<GameObject>();
        private readonly int _maxSize;

        /// <summary>
        /// 池中可用对象数量
        /// </summary>
        public int AvailableCount => _pool.Count;

        /// <summary>
        /// 已借出的对象数量
        /// </summary>
        public int ActiveCount { get; private set; }

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">池对象的父节点</param>
        /// <param name="initialSize">初始预热数量</param>
        /// <param name="maxSize">最大容量（0=无限）</param>
        public ObjectPool(GameObject prefab, Transform parent = null, int initialSize = 0, int maxSize = 0)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;

            // 预热
            for (int i = 0; i < initialSize; i++)
            {
                var obj = CreateNewObject();
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// 从池中获取一个对象
        /// </summary>
        /// <param name="position">初始位置</param>
        /// <param name="rotation">初始旋转</param>
        /// <returns>激活的GameObject</returns>
        public GameObject Get(Vector3 position = default, Quaternion rotation = default)
        {
            GameObject obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();

                // 处理被外部销毁的对象
                if (obj == null)
                {
                    obj = CreateNewObject();
                }
            }
            else
            {
                obj = CreateNewObject();
            }

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            ActiveCount++;
            return obj;
        }

        /// <summary>
        /// 归还对象到池中
        /// </summary>
        /// <param name="obj">要归还的对象</param>
        public void Return(GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.SetParent(_parent);
            ActiveCount--;

            if (_maxSize > 0 && _pool.Count >= _maxSize)
            {
                // 超出最大容量，直接销毁
                Object.Destroy(obj);
                return;
            }

            _pool.Enqueue(obj);
        }

        /// <summary>
        /// 清空对象池，销毁所有缓存对象
        /// </summary>
        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }
            ActiveCount = 0;
        }

        private GameObject CreateNewObject()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.name = _prefab.name;
            return obj;
        }
    }
}
