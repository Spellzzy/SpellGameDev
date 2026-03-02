using System;
using System.Diagnostics;
using UnityEngine;

namespace GameFramework.Log
{
    /// <summary>
    /// 游戏日志系统。
    /// 支持多级别日志、条件编译控制、模块标签过滤。
    /// Release包中可关闭Debug/Info级别日志以提升性能。
    /// </summary>
    public static class GameLogger
    {
        /// <summary>
        /// 日志级别
        /// </summary>
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            None = 99
        }

        /// <summary>
        /// 当前最低输出级别，低于此级别的日志将被忽略
        /// </summary>
        public static LogLevel CurrentLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        /// 是否显示时间戳
        /// </summary>
        public static bool ShowTimestamp { get; set; } = true;

        /// <summary>
        /// 格式化日志消息
        /// </summary>
        private static string Format(string tag, string message)
        {
            if (ShowTimestamp)
            {
                return $"[{DateTime.Now:HH:mm:ss.fff}][{tag}] {message}";
            }
            return $"[{tag}] {message}";
        }

        /// <summary>
        /// 输出Debug级别日志（仅在ENABLE_LOG或UNITY_EDITOR下生效）
        /// </summary>
        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogDebug(string tag, string message)
        {
            if (CurrentLevel <= LogLevel.Debug)
            {
                UnityEngine.Debug.Log(Format(tag, message));
            }
        }

        /// <summary>
        /// 输出Info级别日志（仅在ENABLE_LOG或UNITY_EDITOR下生效）
        /// </summary>
        [Conditional("ENABLE_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogInfo(string tag, string message)
        {
            if (CurrentLevel <= LogLevel.Info)
            {
                UnityEngine.Debug.Log(Format(tag, message));
            }
        }

        /// <summary>
        /// 输出Warning级别日志
        /// </summary>
        public static void LogWarning(string tag, string message)
        {
            if (CurrentLevel <= LogLevel.Warning)
            {
                UnityEngine.Debug.LogWarning(Format(tag, message));
            }
        }

        /// <summary>
        /// 输出Error级别日志
        /// </summary>
        public static void LogError(string tag, string message)
        {
            if (CurrentLevel <= LogLevel.Error)
            {
                UnityEngine.Debug.LogError(Format(tag, message));
            }
        }

        /// <summary>
        /// 输出异常日志
        /// </summary>
        public static void LogException(string tag, Exception exception)
        {
            if (CurrentLevel <= LogLevel.Error)
            {
                UnityEngine.Debug.LogError(Format(tag, $"Exception: {exception.Message}\n{exception.StackTrace}"));
            }
        }
    }
}
