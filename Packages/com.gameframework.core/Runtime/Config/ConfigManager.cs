using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Log;

namespace GameFramework.Config
{
    /// <summary>
    /// 配置管理器。
    /// 从JSON文件加载游戏配置数据，支持热重载。
    /// 配置文件放在 Resources/Configs/ 目录下。
    /// </summary>
    public class ConfigManager : Singleton<ConfigManager>
    {
        private const string TAG = "ConfigManager";
        private const string CONFIG_ROOT = "Configs/";

        // 缓存已加载的配置
        private readonly Dictionary<string, object> _configCache = new Dictionary<string, object>();

        // 缓存原始JSON文本
        private readonly Dictionary<string, string> _rawJsonCache = new Dictionary<string, string>();

        protected override void OnInit()
        {
            GameLogger.LogInfo(TAG, "ConfigManager initialized.");
        }

        /// <summary>
        /// 加载配置文件并反序列化为指定类型
        /// </summary>
        /// <typeparam name="T">配置数据类型</typeparam>
        /// <param name="configName">配置文件名（不含扩展名）</param>
        /// <param name="forceReload">是否强制重新加载</param>
        /// <returns>配置数据实例</returns>
        public T LoadConfig<T>(string configName, bool forceReload = false) where T : class
        {
            string key = typeof(T).Name + "_" + configName;

            if (!forceReload && _configCache.TryGetValue(key, out var cached))
            {
                return cached as T;
            }

            string path = CONFIG_ROOT + configName;
            var textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                GameLogger.LogError(TAG, $"Config file not found: '{path}'.");
                return null;
            }

            try
            {
                var config = JsonUtility.FromJson<T>(textAsset.text);
                _configCache[key] = config;
                _rawJsonCache[configName] = textAsset.text;
                GameLogger.LogInfo(TAG, $"Config '{configName}' loaded as {typeof(T).Name}.");
                return config;
            }
            catch (Exception e)
            {
                GameLogger.LogError(TAG, $"Failed to parse config '{configName}': {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 加载配置列表（JSON数组）。
        /// 由于JsonUtility不直接支持顶层数组，需要包裹类。
        /// </summary>
        /// <typeparam name="T">配置包裹类（需包含列表字段）</typeparam>
        /// <param name="configName">配置文件名</param>
        /// <returns>配置包裹实例</returns>
        public T LoadConfigList<T>(string configName) where T : class
        {
            return LoadConfig<T>(configName);
        }

        /// <summary>
        /// 获取已缓存的原始JSON文本
        /// </summary>
        public string GetRawJson(string configName)
        {
            _rawJsonCache.TryGetValue(configName, out var json);
            return json;
        }

        /// <summary>
        /// 清除指定配置缓存
        /// </summary>
        public void ClearCache(string configName)
        {
            var keysToRemove = new List<string>();
            foreach (var key in _configCache.Keys)
            {
                if (key.Contains(configName))
                {
                    keysToRemove.Add(key);
                }
            }
            foreach (var key in keysToRemove)
            {
                _configCache.Remove(key);
            }
            _rawJsonCache.Remove(configName);
        }

        /// <summary>
        /// 清除所有配置缓存
        /// </summary>
        public void ClearAllCache()
        {
            _configCache.Clear();
            _rawJsonCache.Clear();
        }

        public override void Dispose()
        {
            ClearAllCache();
            base.Dispose();
        }
    }
}
