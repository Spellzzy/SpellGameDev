namespace GameFramework.UI
{
    /// <summary>
    /// UI层级定义，数值越大层级越高。
    /// 控制UI面板的显示层级和排序。
    /// </summary>
    public enum UILayer
    {
        /// <summary>
        /// 背景层（全屏背景、场景UI）
        /// </summary>
        Background = 0,

        /// <summary>
        /// 普通层（主界面、功能面板）
        /// </summary>
        Normal = 100,

        /// <summary>
        /// 弹窗层（确认框、提示框）
        /// </summary>
        Popup = 200,

        /// <summary>
        /// 引导层（新手引导遮罩）
        /// </summary>
        Guide = 300,

        /// <summary>
        /// 顶层（Loading、全局Toast）
        /// </summary>
        Top = 400
    }
}
