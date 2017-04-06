using System.IO;

namespace Faross.Services.Default
{
    public class FileService : IFileService
    {
        public Stream Open(string path)
        {
            return File.Open(path, FileMode.Open);
        }
    }
}