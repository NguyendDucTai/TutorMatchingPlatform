using System.IO;
using System.Threading.Tasks;

namespace TutorPlatform.Application.Common.Interfaces
{
    public interface IPhotoService
    {
        Task<PhotoUploadResult> UploadImageAsync(Stream fileStream, string fileName, string folder = "tutor_platform");
        Task<bool> DeleteImageAsync(string publicId);
    }

    public class PhotoUploadResult
    {
        public bool Success { get; set; }
        public string? Url { get; set; }
        public string? PublicId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
