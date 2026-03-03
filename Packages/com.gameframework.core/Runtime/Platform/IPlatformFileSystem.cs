namespace GameFramework.Platform
{
    /// <summary>
    /// 平台文件系统接口。
    /// 抽象 System.IO / WX.FileSystemManager 等不同平台的文件读写实现。
    /// </summary>
    public interface IPlatformFileSystem
    {
        /// <summary>
        /// 持久化数据根路径
        /// </summary>
        string PersistentDataPath { get; }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        bool FileExists(string path);

        /// <summary>
        /// 读取文件全部文本内容
        /// </summary>
        string ReadAllText(string path);

        /// <summary>
        /// 写入文本内容到文件（覆盖）
        /// </summary>
        void WriteAllText(string path, string content);

        /// <summary>
        /// 删除文件
        /// </summary>
        void DeleteFile(string path);

        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        bool DirectoryExists(string path);

        /// <summary>
        /// 创建目录（含父目录）
        /// </summary>
        void CreateDirectory(string path);

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <param name="recursive">是否递归删除子目录和文件</param>
        void DeleteDirectory(string path, bool recursive);
    }
}
