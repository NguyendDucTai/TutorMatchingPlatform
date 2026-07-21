using System.Threading.Tasks;

namespace TutorMatchingPlatform.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(FileUploadDto file, string subFolder, string[] allowedExtensions, int maxMegaBytes);
    }
}
