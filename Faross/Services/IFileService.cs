using System.IO;

namespace Faross.Services
{
    public interface IFileService
    {
        Stream Open(string path);
    }
}