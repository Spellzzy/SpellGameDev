using System;
using System.Text;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Event;
using GameFramework.Log;
using GameFramework.Platform;

namespace GameFramework.Save
{
    /// <summary>
    /// 存档管理器。
    /// 支持多槽位存档、JSON序列化、可选XOR加密。
    /// 通过 PlatformManager.FileSystem 实现跨平台文件读写，
    /// 无需关心底层是 System.IO 还是微信文件系统。
    /// </summary>
    public class SaveManager : Singleton<SaveManager>
    {
        private const string TAG = "SaveManager";
        private const string SAVE_DIR = "Saves";
        private const string SAVE_EXT = ".sav";

        /// <summary>
        /// 是否启用加密
        /// </summary>
        public bool EnableEncryption { get; set; } = false;

        /// <summary>
        /// 加密密钥（16/24/32字节）
        /// </summary>
        public string EncryptionKey { get; set; } = "YourGameKey12345";

        /// <summary>
        /// 当前活跃槽位索引
        /// </summary>
        public int CurrentSlot { get; set; } = 0;

        /// <summary>
        /// 最大存档槽位数
        /// </summary>
        public int MaxSlots { get; set; } = 5;

        private IPlatformFileSystem _fs;

        private string SaveDirectory
        {
            get
            {
                string basePath = _fs?.PersistentDataPath ?? Application.persistentDataPath;
                return $"{basePath}/{SAVE_DIR}";
            }
        }

        protected override void OnInit()
        {
            _fs = PlatformManager.Instance.FileSystem;

            if (!_fs.DirectoryExists(SaveDirectory))
            {
                _fs.CreateDirectory(SaveDirectory);
            }
            GameLogger.LogInfo(TAG, $"SaveManager initialized. Path: {SaveDirectory}");
        }

        /// <summary>
        /// 保存数据到指定槽位
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">数据对象</param>
        /// <param name="slot">槽位索引（-1使用当前槽位）</param>
        /// <param name="fileName">自定义文件名（null使用默认）</param>
        /// <returns>是否成功</returns>
        public bool Save<T>(T data, int slot = -1, string fileName = null) where T : class
        {
            if (slot < 0) slot = CurrentSlot;

            string path = GetSavePath(slot, fileName);

            try
            {
                string json = JsonUtility.ToJson(data, true);

                if (EnableEncryption)
                {
                    json = Encrypt(json);
                }

                _fs.WriteAllText(path, json);
                GameLogger.LogInfo(TAG, $"Data saved to slot {slot}: {path}");
                EventSystem.Instance.Publish(GameEvents.SAVE_COMPLETE, this);
                return true;
            }
            catch (Exception e)
            {
                GameLogger.LogError(TAG, $"Save failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从指定槽位加载数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="slot">槽位索引（-1使用当前槽位）</param>
        /// <param name="fileName">自定义文件名（null使用默认）</param>
        /// <returns>数据对象，失败返回null</returns>
        public T Load<T>(int slot = -1, string fileName = null) where T : class
        {
            if (slot < 0) slot = CurrentSlot;

            string path = GetSavePath(slot, fileName);

            if (!_fs.FileExists(path))
            {
                GameLogger.LogWarning(TAG, $"Save file not found: {path}");
                return null;
            }

            try
            {
                string json = _fs.ReadAllText(path);

                if (EnableEncryption)
                {
                    json = Decrypt(json);
                }

                var data = JsonUtility.FromJson<T>(json);
                GameLogger.LogInfo(TAG, $"Data loaded from slot {slot}.");
                EventSystem.Instance.Publish(GameEvents.LOAD_COMPLETE, this);
                return data;
            }
            catch (Exception e)
            {
                GameLogger.LogError(TAG, $"Load failed: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 检查槽位是否有存档
        /// </summary>
        public bool HasSave(int slot = -1, string fileName = null)
        {
            if (slot < 0) slot = CurrentSlot;
            return _fs.FileExists(GetSavePath(slot, fileName));
        }

        /// <summary>
        /// 删除指定槽位存档
        /// </summary>
        public bool DeleteSave(int slot = -1, string fileName = null)
        {
            if (slot < 0) slot = CurrentSlot;
            string path = GetSavePath(slot, fileName);

            if (_fs.FileExists(path))
            {
                _fs.DeleteFile(path);
                GameLogger.LogInfo(TAG, $"Save deleted: {path}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 删除所有存档
        /// </summary>
        public void DeleteAllSaves()
        {
            if (_fs.DirectoryExists(SaveDirectory))
            {
                _fs.DeleteDirectory(SaveDirectory, true);
                _fs.CreateDirectory(SaveDirectory);
                GameLogger.LogInfo(TAG, "All saves deleted.");
            }
        }

        /// <summary>
        /// 获取存档文件路径
        /// </summary>
        private string GetSavePath(int slot, string fileName = null)
        {
            string name = string.IsNullOrEmpty(fileName) ? $"save_slot{slot}" : fileName;
            return $"{SaveDirectory}/{name}{SAVE_EXT}";
        }

        #region 简单XOR加密（可替换为AES）

        private string Encrypt(string plainText)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < plainText.Length; i++)
            {
                sb.Append((char)(plainText[i] ^ EncryptionKey[i % EncryptionKey.Length]));
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        private string Decrypt(string encryptedText)
        {
            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encryptedText));
            var sb = new StringBuilder();
            for (int i = 0; i < decoded.Length; i++)
            {
                sb.Append((char)(decoded[i] ^ EncryptionKey[i % EncryptionKey.Length]));
            }
            return sb.ToString();
        }

        #endregion

        public override void Dispose()
        {
            _fs = null;
            base.Dispose();
        }
    }
}
