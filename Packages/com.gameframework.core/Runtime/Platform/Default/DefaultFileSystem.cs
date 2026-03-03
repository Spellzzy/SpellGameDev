using System.IO;
using System.Text;
using UnityEngine;

namespace GameFramework.Platform
{
    /// <summary>
    /// 默认文件系统实现。
    /// 基于 System.IO，适用于 Editor / Standalone / 移动端原生平台。
    /// </summary>
    public class DefaultFileSystem : IPlatformFileSystem
    {
        /// <summary>
        /// 持久化数据根路径
        /// </summary>
        public string PersistentDataPath => Application.persistentDataPath;

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// 读取文件全部文本内容
        /// </summary>
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }

        /// <summary>
        /// 写入文本内容到文件（覆盖）
        /// </summary>
        public void WriteAllText(string path, string content)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(path, content, Encoding.UTF8);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        public void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// 创建目录（含父目录）
        /// </summary>
        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        public void DeleteDirectory(string path, bool recursive)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }
    }
}
