// ============================================================================
// 微信小游戏键值存储实现（预留）
// 导入微信 Unity SDK 后，添加 WEIXINMINIGAME 条件编译符号即可激活。
// SDK 地址：https://github.com/wechat-miniprogram/minigame-tuanjie-transform-sdk
// ============================================================================

#if WEIXINMINIGAME

using WeChatWASM;

namespace GameFramework.Platform
{
    /// <summary>
    /// 微信小游戏键值存储实现。
    /// 基于 WX.StorageSetStringSync / WX.StorageGetStringSync。
    /// </summary>
    public class WXStorage : IPlatformStorage
    {
        /// <summary>
        /// 存储字符串
        /// </summary>
        public void SetString(string key, string value)
        {
            WX.StorageSetStringSync(key, value);
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        public string GetString(string key, string defaultValue = "")
        {
            string result = WX.StorageGetStringSync(key, "");
            return string.IsNullOrEmpty(result) ? defaultValue : result;
        }

        /// <summary>
        /// 存储整数
        /// </summary>
        public void SetInt(string key, int value)
        {
            WX.StorageSetIntSync(key, value);
        }

        /// <summary>
        /// 读取整数
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            return WX.StorageGetIntSync(key, defaultValue);
        }

        /// <summary>
        /// 存储浮点数
        /// </summary>
        public void SetFloat(string key, float value)
        {
            WX.StorageSetFloatSync(key, value);
        }

        /// <summary>
        /// 读取浮点数
        /// </summary>
        public float GetFloat(string key, float defaultValue = 0f)
        {
            return WX.StorageGetFloatSync(key, defaultValue);
        }

        /// <summary>
        /// 检查键是否存在
        /// </summary>
        public bool HasKey(string key)
        {
            // 微信 SDK 没有直接的 HasKey，通过尝试获取判断
            string result = WX.StorageGetStringSync(key, "");
            return !string.IsNullOrEmpty(result);
        }

        /// <summary>
        /// 删除指定键
        /// </summary>
        public void DeleteKey(string key)
        {
            WX.StorageRemoveSync(key);
        }

        /// <summary>
        /// 删除所有键值对
        /// </summary>
        public void DeleteAll()
        {
            WX.StorageClearSync();
        }

        /// <summary>
        /// 持久化数据（微信存储为同步写入，无需额外 flush）
        /// </summary>
        public void Flush()
        {
            // 微信 StorageSetSync 本身就是同步持久化，无需额外操作
        }
    }
}

#endif
