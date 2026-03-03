namespace GameFramework.Platform
{
    /// <summary>
    /// 平台类型枚举。
    /// 用于标识当前运行平台，决定使用哪套平台实现。
    /// </summary>
    public enum PlatformType
    {
        /// <summary>
        /// 默认平台（Editor / Standalone / 移动端原生）
        /// </summary>
        Default = 0,

        /// <summary>
        /// 微信小游戏
        /// </summary>
        WXMiniGame = 1
    }
}
