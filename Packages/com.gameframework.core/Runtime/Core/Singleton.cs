using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// 普通C#类的单例基类（非MonoBehaviour）。
    /// 适用于纯逻辑管理器，不依赖Unity生命周期。
    /// </summary>
    /// <typeparam name="T">子类类型</typeparam>
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                            _instance.OnInit();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 实例是否存在（不会触发自动创建）
        /// </summary>
        public static bool HasInstance => _instance != null;

        /// <summary>
        /// 初始化回调，子类可重写
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 销毁单例实例
        /// </summary>
        public virtual void Dispose()
        {
            _instance = null;
        }
    }
}
