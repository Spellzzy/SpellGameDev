using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Log;

namespace GameFramework.Utils
{
    /// <summary>
    /// 定时器管理器。
    /// 提供延时调用、重复调用、可取消的定时任务。
    /// 由 GameManager 在 Update 中驱动。
    /// </summary>
    public class TimerManager : MonoSingleton<TimerManager>
    {
        private const string TAG = "TimerManager";

        private readonly List<TimerTask> _tasks = new List<TimerTask>();
        private readonly List<TimerTask> _pendingAdd = new List<TimerTask>();
        private readonly List<int> _pendingRemove = new List<int>();
        private int _nextId = 1;
        private bool _isUpdating = false;

        /// <summary>
        /// 定时任务
        /// </summary>
        private class TimerTask
        {
            public int Id;
            public float Delay;
            public float Interval;
            public int RepeatCount; // -1 = 无限
            public Action Callback;
            public bool UseRealTime;
            public float ElapsedTime;
            public int ExecutedCount;
            public bool IsActive;
        }

        /// <summary>
        /// 延迟调用（一次性）
        /// </summary>
        /// <param name="delay">延迟秒数</param>
        /// <param name="callback">回调方法</param>
        /// <param name="useRealTime">是否使用真实时间（不受TimeScale影响）</param>
        /// <returns>任务ID，可用于取消</returns>
        public int Delay(float delay, Action callback, bool useRealTime = false)
        {
            return Schedule(delay, 0f, 1, callback, useRealTime);
        }

        /// <summary>
        /// 重复调用
        /// </summary>
        /// <param name="interval">间隔秒数</param>
        /// <param name="repeatCount">重复次数（-1 = 无限）</param>
        /// <param name="callback">回调方法</param>
        /// <param name="initialDelay">首次延迟</param>
        /// <param name="useRealTime">是否使用真实时间</param>
        /// <returns>任务ID</returns>
        public int Repeat(float interval, int repeatCount, Action callback, float initialDelay = 0f, bool useRealTime = false)
        {
            return Schedule(initialDelay > 0 ? initialDelay : interval, interval, repeatCount, callback, useRealTime);
        }

        /// <summary>
        /// 每帧调用（下一帧执行一次）
        /// </summary>
        public int NextFrame(Action callback)
        {
            return Delay(0f, callback);
        }

        /// <summary>
        /// 取消定时任务
        /// </summary>
        public void Cancel(int taskId)
        {
            if (_isUpdating)
            {
                _pendingRemove.Add(taskId);
            }
            else
            {
                _tasks.RemoveAll(t => t.Id == taskId);
            }
        }

        /// <summary>
        /// 取消所有任务
        /// </summary>
        public void CancelAll()
        {
            _tasks.Clear();
            _pendingAdd.Clear();
            _pendingRemove.Clear();
        }

        private int Schedule(float delay, float interval, int repeatCount, Action callback, bool useRealTime)
        {
            var task = new TimerTask
            {
                Id = _nextId++,
                Delay = delay,
                Interval = interval,
                RepeatCount = repeatCount,
                Callback = callback,
                UseRealTime = useRealTime,
                ElapsedTime = 0f,
                ExecutedCount = 0,
                IsActive = true
            };

            if (_isUpdating)
            {
                _pendingAdd.Add(task);
            }
            else
            {
                _tasks.Add(task);
            }

            return task.Id;
        }

        private void Update()
        {
            _isUpdating = true;

            for (int i = _tasks.Count - 1; i >= 0; i--)
            {
                var task = _tasks[i];
                if (!task.IsActive) continue;

                float dt = task.UseRealTime ? Time.unscaledDeltaTime : Time.deltaTime;
                task.ElapsedTime += dt;

                float targetTime = task.ExecutedCount == 0 ? task.Delay : task.Interval;

                if (task.ElapsedTime >= targetTime)
                {
                    task.ElapsedTime -= targetTime;
                    task.ExecutedCount++;

                    try
                    {
                        task.Callback?.Invoke();
                    }
                    catch (Exception e)
                    {
                        GameLogger.LogError(TAG, $"Timer callback error (id={task.Id}): {e.Message}");
                    }

                    // 检查是否完成
                    if (task.RepeatCount > 0 && task.ExecutedCount >= task.RepeatCount)
                    {
                        _tasks.RemoveAt(i);
                    }
                }
            }

            _isUpdating = false;

            // 处理待添加
            if (_pendingAdd.Count > 0)
            {
                _tasks.AddRange(_pendingAdd);
                _pendingAdd.Clear();
            }

            // 处理待移除
            if (_pendingRemove.Count > 0)
            {
                foreach (var id in _pendingRemove)
                {
                    _tasks.RemoveAll(t => t.Id == id);
                }
                _pendingRemove.Clear();
            }
        }
    }
}
