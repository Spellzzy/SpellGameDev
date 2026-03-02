namespace GameFramework.Event
{
    /// <summary>
    /// 游戏事件参数基类。
    /// 所有自定义事件参数应继承此类。
    /// </summary>
    public abstract class GameEventArgs
    {
        /// <summary>
        /// 事件ID，用于标识事件类型
        /// </summary>
        public abstract string EventId { get; }
    }
}
