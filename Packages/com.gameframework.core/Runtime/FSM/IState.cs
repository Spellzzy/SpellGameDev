namespace GameFramework.FSM
{
    /// <summary>
    /// 状态接口，所有FSM状态需实现此接口。
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 进入状态时调用
        /// </summary>
        /// <param name="prevState">上一个状态（可能为null）</param>
        void OnEnter(IState prevState);

        /// <summary>
        /// 每帧更新
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// 固定时间步长更新（物理相关）
        /// </summary>
        void OnFixedUpdate();

        /// <summary>
        /// 退出状态时调用
        /// </summary>
        /// <param name="nextState">下一个状态</param>
        void OnExit(IState nextState);
    }
}
