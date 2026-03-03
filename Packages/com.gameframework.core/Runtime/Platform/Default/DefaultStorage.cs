using UnityEngine;

namespace GameFramework.Platform
{
    /// <summary>
    /// 默认键值存储实现。
    /// 基于 PlayerPrefs，适用于 Editor / Standalone / 移动端原生平台。
    /// </summary>
    public class DefaultStorage : IPlatformStorage
    {
        /// <summary>
        /// 存储字符串
        /// </summary>
        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        public string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        /// <summary>
        /// 存储整数
        /// </summary>
        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        /// <summary>
        /// 读取整数
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        /// <summary>
        /// 存储浮点数
        /// </summary>
        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        /// <summary>
        /// 读取浮点数
        /// </summary>
        public float GetFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        /// <summary>
        /// 检查键是否存在
        /// </summary>
        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// 删除指定键
        /// </summary>
        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        /// <summary>
        /// 删除所有键值对
        /// </summary>
        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        /// <summary>
        /// 持久化数据到磁盘
        /// </summary>
        public void Flush()
        {
            PlayerPrefs.Save();
        }
    }
}
