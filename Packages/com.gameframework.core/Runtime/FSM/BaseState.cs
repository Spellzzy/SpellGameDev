namespace GameFramework.FSM
{
    /// <summary>
    /// 状态基类，提供IState的默认空实现，
    /// 子类只需覆写关心的方法即可。
    /// </summary>
    public abstract class BaseState : IState
    {
        /// <summary>
        /// 所属状态机引用
        /// </summary>
        protected StateMachine Owner { get; private set; }

        public BaseState(StateMachine owner)
        {
            Owner = owner;
        }

        public virtual void OnEnter(IState prevState) { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnExit(IState nextState) { }
    }
}
