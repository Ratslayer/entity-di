using System.Threading.Tasks;

namespace BB
{
    public interface IFileSystem
    {
        Task Write(WriteFileContext context);
        Task<T> Read<T>(ReadFileContext context);
        string GetFullPath(string localPath);
    }
}