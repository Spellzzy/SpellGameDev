// ============================================================================
// 微信小游戏文件系统实现（预留）
// 导入微信 Unity SDK 后，添加 WEIXINMINIGAME 条件编译符号即可激活。
// ============================================================================

#if WEIXINMINIGAME

using WeChatWASM;

namespace GameFramework.Platform
{
    /// <summary>
    /// 微信小游戏文件系统实现。
    /// 基于 WX.GetFileSystemManager()，提供文件读写能力。
    /// 微信小游戏的用户文件存储路径为 wx.env.USER_DATA_PATH。
    /// </summary>
    public class WXFileSystem : IPlatformFileSystem
    {
        private WXFileSystemManager _fs;

        /// <summary>
        /// 持久化数据根路径（微信用户数据路径）
        /// </summary>
        public string PersistentDataPath => WX.env.USER_DATA_PATH;

        /// <summary>
        /// 初始化文件系统管理器
        /// </summary>
        public WXFileSystem()
        {
            _fs = WX.GetFileSystemManager();
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public bool FileExists(string path)
        {
            string result = _fs.AccessSync(path);
            return result == "access:ok";
        }

        /// <summary>
        /// 读取文件全部文本内容
        /// </summary>
        public string ReadAllText(string path)
        {
            return _fs.ReadFileSync(path, "utf8");
        }

        /// <summary>
        /// 写入文本内容到文件（覆盖）
        /// </summary>
        public void WriteAllText(string path, string content)
        {
            // 确保父目录存在
            string dir = path.Substring(0, path.LastIndexOf('/'));
            if (!string.IsNullOrEmpty(dir))
            {
                string accessResult = _fs.AccessSync(dir);
                if (accessResult != "access:ok")
                {
                    _fs.MkdirSync(dir, true);
                }
            }
            _fs.WriteFileSync(path, content, "utf8");
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        public void DeleteFile(string path)
        {
            string result = _fs.AccessSync(path);
            if (result == "access:ok")
            {
                _fs.UnlinkSync(path);
            }
        }

        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        public bool DirectoryExists(string path)
        {
            string result = _fs.AccessSync(path);
            return result == "access:ok";
        }

        /// <summary>
        /// 创建目录（含父目录）
        /// </summary>
        public void CreateDirectory(string path)
        {
            string result = _fs.AccessSync(path);
            if (result != "access:ok")
            {
                _fs.MkdirSync(path, true);
            }
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        public void DeleteDirectory(string path, bool recursive)
        {
            string result = _fs.AccessSync(path);
            if (result == "access:ok")
            {
                _fs.RmdirSync(path, recursive);
            }
        }
    }
}

#endif
