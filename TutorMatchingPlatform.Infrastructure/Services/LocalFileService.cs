using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TutorMatchingPlatform.Application.Interfaces;

namespace TutorMatchingPlatform.Infrastructure.Services
{
    public class LocalFileService : IFileService
    {
        public async Task<string> UploadFileAsync(FileUploadDto file, string subFolder, string[] allowedExtensions, int maxMegaBytes)
        {
            if (file == null || file.Content.Length == 0)
                throw new Exception("File is empty.");

            if (file.Content.Length > maxMegaBytes * 1024 * 1024)
                throw new Exception($"File size exceeds the limit of {maxMegaBytes} MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new Exception($"Invalid file extension. Allowed: {string.Join(", ", allowedExtensions)}");

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.Content.CopyToAsync(fileStream);
            }

            return $"/uploads/{subFolder}/{uniqueFileName}";
        }
    }
}
