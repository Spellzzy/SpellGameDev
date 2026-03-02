using UnityEngine;

namespace GameFramework.UI
{
    /// <summary>
    /// UI面板基类，所有UI面板应继承此类。
    /// 提供标准的面板生命周期回调。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : MonoBehaviour
    {
        /// <summary>
        /// 面板所属层级
        /// </summary>
        public virtual UILayer Layer => UILayer.Normal;

        /// <summary>
        /// 面板唯一标识（默认使用类名）
        /// </summary>
        public virtual string PanelName => GetType().Name;

        /// <summary>
        /// 是否在打开其他面板时自动隐藏
        /// </summary>
        public virtual bool HideOnOtherOpen => false;

        private CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        /// <summary>
        /// 面板初始化（仅在首次创建时调用一次）
        /// </summary>
        /// <param name="args">初始化参数</param>
        public virtual void OnInit(object args = null) { }

        /// <summary>
        /// 面板打开时调用
        /// </summary>
        /// <param name="args">打开参数</param>
        public virtual void OnOpen(object args = null)
        {
            gameObject.SetActive(true);
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 面板关闭时调用
        /// </summary>
        public virtual void OnClose()
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 面板被暂时隐藏时调用（被其他面板覆盖）
        /// </summary>
        public virtual void OnHide()
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// 面板从隐藏恢复显示时调用
        /// </summary>
        public virtual void OnResume()
        {
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 面板刷新数据
        /// </summary>
        /// <param name="args">刷新参数</param>
        public virtual void OnRefresh(object args = null) { }

        /// <summary>
        /// 面板销毁时调用
        /// </summary>
        public virtual void OnDispose() { }
    }
}
