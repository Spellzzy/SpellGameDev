using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// MonoBehaviour单例基类。
    /// 适用于需要Unity生命周期（Update/Coroutine等）的管理器。
    /// 跨场景不销毁，保证全局唯一。
    /// </summary>
    /// <typeparam name="T">子类类型</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// 获取单例实例。
        /// 如果场景中不存在则自动创建。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[MonoSingleton] '{typeof(T)}' already destroyed on application quit. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            var go = new GameObject($"[{typeof(T).Name}]");
                            _instance = go.AddComponent<T>();
                            DontDestroyOnLoad(go);
                        }
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// 实例是否存在（不会触发自动创建）
        /// </summary>
        public static bool HasInstance => _instance != null;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnInit();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[MonoSingleton] Duplicate '{typeof(T)}' detected. Destroying...");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化回调，子类重写此方法代替Awake
        /// </summary>
        protected virtual void OnInit() { }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}
