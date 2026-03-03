namespace GameFramework.Platform
{
    /// <summary>
    /// 平台键值对存储接口。
    /// 抽象 PlayerPrefs / WX.StorageSetSync 等不同平台的键值存储实现。
    /// </summary>
    public interface IPlatformStorage
    {
        /// <summary>
        /// 存储字符串
        /// </summary>
        void SetString(string key, string value);

        /// <summary>
        /// 读取字符串
        /// </summary>
        string GetString(string key, string defaultValue = "");

        /// <summary>
        /// 存储整数
        /// </summary>
        void SetInt(string key, int value);

        /// <summary>
        /// 读取整数
        /// </summary>
        int GetInt(string key, int defaultValue = 0);

        /// <summary>
        /// 存储浮点数
        /// </summary>
        void SetFloat(string key, float value);

        /// <summary>
        /// 读取浮点数
        /// </summary>
        float GetFloat(string key, float defaultValue = 0f);

        /// <summary>
        /// 检查键是否存在
        /// </summary>
        bool HasKey(string key);

        /// <summary>
        /// 删除指定键
        /// </summary>
        void DeleteKey(string key);

        /// <summary>
        /// 删除所有键值对
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// 持久化数据（如 PlayerPrefs.Save）
        /// </summary>
        void Flush();
    }
}
