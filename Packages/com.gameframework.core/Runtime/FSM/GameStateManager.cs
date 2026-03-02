using GameFramework.Core;
using GameFramework.Event;
using GameFramework.Log;

namespace GameFramework.FSM
{
    /// <summary>
    /// 游戏全局状态管理器。
    /// 管理游戏的顶层流程状态（如 启动->主菜单->游戏中->暂停->结算）。
    /// </summary>
    public class GameStateManager : Singleton<GameStateManager>
    {
        private const string TAG = "GameStateManager";

        /// <summary>
        /// 内部状态机实例
        /// </summary>
        public StateMachine FSM { get; private set; }

        protected override void OnInit()
        {
            FSM = new StateMachine("GameState");
            FSM.OnStateChanged += OnGameStateChanged;
            GameLogger.LogInfo(TAG, "GameStateManager initialized.");
        }

        /// <summary>
        /// 注册游戏状态
        /// </summary>
        public void AddState<T>(T state) where T : IState
        {
            FSM.AddState(state);
        }

        /// <summary>
        /// 启动游戏状态机
        /// </summary>
        public void Start<T>() where T : IState
        {
            FSM.Start<T>();
        }

        /// <summary>
        /// 切换游戏状态
        /// </summary>
        public void ChangeState<T>() where T : IState
        {
            FSM.ChangeState<T>();
        }

        /// <summary>
        /// 每帧驱动（由GameManager调用）
        /// </summary>
        public void Update()
        {
            FSM.Update();
        }

        /// <summary>
        /// 固定更新
        /// </summary>
        public void FixedUpdate()
        {
            FSM.FixedUpdate();
        }

        private void OnGameStateChanged(IState prevState, IState newState)
        {
            var args = new GameStateChangedEventArgs(prevState, newState);
            EventSystem.Instance.Publish(GameEvents.GAME_STATE_CHANGED, this, args);
        }

        public override void Dispose()
        {
            FSM?.Stop();
            FSM = null;
            base.Dispose();
        }
    }

    /// <summary>
    /// 游戏状态切换事件参数
    /// </summary>
    public class GameStateChangedEventArgs : GameEventArgs
    {
        public override string EventId => GameEvents.GAME_STATE_CHANGED;

        public IState PreviousState { get; }
        public IState NewState { get; }

        public GameStateChangedEventArgs(IState prevState, IState newState)
        {
            PreviousState = prevState;
            NewState = newState;
        }
    }
}
