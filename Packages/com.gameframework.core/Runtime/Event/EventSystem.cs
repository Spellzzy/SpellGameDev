using System;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.Event
{
    /// <summary>
    /// 全局事件系统，基于发布-订阅模式实现模块间解耦通信。
    /// 支持带参数和无参数两种事件。
    /// </summary>
    public class EventSystem : Singleton<EventSystem>
    {
        /// <summary>
        /// 事件回调委托（带参数）
        /// </summary>
        public delegate void EventHandler(object sender, GameEventArgs args);

        /// <summary>
        /// 事件回调委托（无参数）
        /// </summary>
        public delegate void EventHandlerNoArgs(object sender);

        private readonly Dictionary<string, List<EventHandler>> _eventHandlers = new Dictionary<string, List<EventHandler>>();
        private readonly Dictionary<string, List<EventHandlerNoArgs>> _eventHandlersNoArgs = new Dictionary<string, List<EventHandlerNoArgs>>();

        #region 带参数事件

        /// <summary>
        /// 订阅事件（带参数）
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="handler">回调方法</param>
        public void Subscribe(string eventName, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            if (!_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] = new List<EventHandler>();
            }

            if (!_eventHandlers[eventName].Contains(handler))
            {
                _eventHandlers[eventName].Add(handler);
            }
        }

        /// <summary>
        /// 取消订阅事件（带参数）
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="handler">回调方法</param>
        public void Unsubscribe(string eventName, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            if (_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(eventName);
                }
            }
        }

        /// <summary>
        /// 触发事件（带参数）
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="sender">事件发送者</param>
        /// <param name="args">事件参数</param>
        public void Publish(string eventName, object sender, GameEventArgs args)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            if (_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                // 复制列表以避免回调中修改集合
                var handlersCopy = new List<EventHandler>(handlers);
                foreach (var handler in handlersCopy)
                {
                    try
                    {
                        handler?.Invoke(sender, args);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"[EventSystem] Error in handler for '{eventName}': {e}");
                    }
                }
            }
        }

        #endregion

        #region 无参数事件

        /// <summary>
        /// 订阅事件（无参数）
        /// </summary>
        public void Subscribe(string eventName, EventHandlerNoArgs handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            if (!_eventHandlersNoArgs.ContainsKey(eventName))
            {
                _eventHandlersNoArgs[eventName] = new List<EventHandlerNoArgs>();
            }

            if (!_eventHandlersNoArgs[eventName].Contains(handler))
            {
                _eventHandlersNoArgs[eventName].Add(handler);
            }
        }

        /// <summary>
        /// 取消订阅事件（无参数）
        /// </summary>
        public void Unsubscribe(string eventName, EventHandlerNoArgs handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;

            if (_eventHandlersNoArgs.TryGetValue(eventName, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _eventHandlersNoArgs.Remove(eventName);
                }
            }
        }

        /// <summary>
        /// 触发事件（无参数）
        /// </summary>
        public void Publish(string eventName, object sender = null)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            if (_eventHandlersNoArgs.TryGetValue(eventName, out var handlers))
            {
                var handlersCopy = new List<EventHandlerNoArgs>(handlers);
                foreach (var handler in handlersCopy)
                {
                    try
                    {
                        handler?.Invoke(sender);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"[EventSystem] Error in handler for '{eventName}': {e}");
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 清除所有事件订阅
        /// </summary>
        public void Clear()
        {
            _eventHandlers.Clear();
            _eventHandlersNoArgs.Clear();
        }

        /// <summary>
        /// 清除指定事件的所有订阅
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void Clear(string eventName)
        {
            _eventHandlers.Remove(eventName);
            _eventHandlersNoArgs.Remove(eventName);
        }

        public override void Dispose()
        {
            Clear();
            base.Dispose();
        }
    }
}
