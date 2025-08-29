using System;
using System.IO;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Interface to control file system operations.
    /// </summary>
    public interface IFileSystem
    {
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
        bool FileExists(string path);
        void DeleteFile(string path);
        Stream OpenWrite(string path);
        byte[] ReadAllBytes(string path);
        void WriteAllBytes(string path, byte[] data);
    }

    /// <summary>
    /// Uses <see cref="File"/> and <see cref="Directory"/> of Unity's file system.
    /// </summary>
    [Serializable]
    public class UnityFileSystem : IFileSystem
    {
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public bool FileExists(string path) => File.Exists(path);
        public void DeleteFile(string path) => File.Delete(path);
        public Stream OpenWrite(string path) => File.Open(path, FileMode.Create);
        public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);
        public void WriteAllBytes(string path, byte[] data) => File.WriteAllBytes(path, data);
    }
}