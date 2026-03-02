using System;
using System.Collections.Generic;
using GameFramework.Log;

namespace GameFramework.FSM
{
    /// <summary>
    /// 通用有限状态机。
    /// 可用于游戏全局状态、角色状态、AI状态等任何需要状态切换的场景。
    /// </summary>
    public class StateMachine
    {
        private const string TAG = "StateMachine";

        private readonly string _name;
        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();

        /// <summary>
        /// 当前状态
        /// </summary>
        public IState CurrentState { get; private set; }

        /// <summary>
        /// 上一个状态
        /// </summary>
        public IState PreviousState { get; private set; }

        /// <summary>
        /// 状态机是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 状态切换事件
        /// </summary>
        public event Action<IState, IState> OnStateChanged;

        public StateMachine(string name = "Default")
        {
            _name = name;
        }

        /// <summary>
        /// 注册一个状态
        /// </summary>
        /// <typeparam name="T">状态类型</typeparam>
        /// <param name="state">状态实例</param>
        public void AddState<T>(T state) where T : IState
        {
            var type = typeof(T);
            if (_states.ContainsKey(type))
            {
                GameLogger.LogWarning(TAG, $"[{_name}] State '{type.Name}' already registered, overwriting.");
            }
            _states[type] = state;
        }

        /// <summary>
        /// 获取已注册的状态
        /// </summary>
        public T GetState<T>() where T : IState
        {
            if (_states.TryGetValue(typeof(T), out var state))
            {
                return (T)state;
            }
            return default;
        }

        /// <summary>
        /// 启动状态机，进入初始状态
        /// </summary>
        public void Start<T>() where T : IState
        {
            if (IsRunning)
            {
                GameLogger.LogWarning(TAG, $"[{_name}] Already running.");
                return;
            }

            var type = typeof(T);
            if (!_states.TryGetValue(type, out var state))
            {
                GameLogger.LogError(TAG, $"[{_name}] State '{type.Name}' not registered.");
                return;
            }

            IsRunning = true;
            CurrentState = state;
            GameLogger.LogInfo(TAG, $"[{_name}] Start -> {type.Name}");
            CurrentState.OnEnter(null);
        }

        /// <summary>
        /// 切换到指定状态
        /// </summary>
        public void ChangeState<T>() where T : IState
        {
            if (!IsRunning)
            {
                GameLogger.LogWarning(TAG, $"[{_name}] Not running, cannot change state.");
                return;
            }

            var type = typeof(T);
            if (!_states.TryGetValue(type, out var nextState))
            {
                GameLogger.LogError(TAG, $"[{_name}] State '{type.Name}' not registered.");
                return;
            }

            if (CurrentState == nextState)
            {
                GameLogger.LogDebug(TAG, $"[{_name}] Already in '{type.Name}', skipping.");
                return;
            }

            var prevState = CurrentState;
            prevState?.OnExit(nextState);

            PreviousState = prevState;
            CurrentState = nextState;
            CurrentState.OnEnter(prevState);

            GameLogger.LogInfo(TAG, $"[{_name}] {prevState?.GetType().Name} -> {type.Name}");
            OnStateChanged?.Invoke(prevState, CurrentState);
        }

        /// <summary>
        /// 每帧更新（需在外部驱动调用）
        /// </summary>
        public void Update()
        {
            if (IsRunning)
            {
                CurrentState?.OnUpdate();
            }
        }

        /// <summary>
        /// 固定更新
        /// </summary>
        public void FixedUpdate()
        {
            if (IsRunning)
            {
                CurrentState?.OnFixedUpdate();
            }
        }

        /// <summary>
        /// 停止状态机
        /// </summary>
        public void Stop()
        {
            if (!IsRunning) return;

            CurrentState?.OnExit(null);
            PreviousState = CurrentState;
            CurrentState = null;
            IsRunning = false;
            GameLogger.LogInfo(TAG, $"[{_name}] Stopped.");
        }
    }
}
