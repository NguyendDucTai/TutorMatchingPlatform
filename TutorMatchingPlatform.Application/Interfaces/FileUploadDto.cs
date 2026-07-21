using System.IO;

namespace TutorMatchingPlatform.Application.Interfaces
{
    public class FileUploadDto
    {
        public Stream Content { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}
