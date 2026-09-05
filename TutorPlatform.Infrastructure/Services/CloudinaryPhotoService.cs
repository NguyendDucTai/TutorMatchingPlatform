using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Infrastructure.Configurations;

namespace TutorPlatform.Infrastructure.Services
{
    public class CloudinaryPhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryPhotoService(IOptions<CloudinarySettings> config)
        {
            var settings = config.Value;
            var cloudName = settings.CloudName;
            var apiKey = settings.ApiKey;
            var apiSecret = settings.ApiSecret;

            // Cấu hình được nạp tự động qua IOptions từ appsettings.Development.json (Local) 
            // hoặc Environment Variables trên Azure App Service (CloudinarySettings__CloudName, ...)
            if (string.IsNullOrWhiteSpace(cloudName) || cloudName == "YOUR_CLOUDINARY_CLOUD_NAME")
            {
                cloudName = Environment.GetEnvironmentVariable("CloudinarySettings__CloudName") ?? string.Empty;
            }
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_CLOUDINARY_API_KEY")
            {
                apiKey = Environment.GetEnvironmentVariable("CloudinarySettings__ApiKey") ?? string.Empty;
            }
            if (string.IsNullOrWhiteSpace(apiSecret) || apiSecret == "YOUR_CLOUDINARY_API_SECRET")
            {
                apiSecret = Environment.GetEnvironmentVariable("CloudinarySettings__ApiSecret") ?? string.Empty;
            }

            var acc = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(acc);
            _cloudinary.Api.Secure = true;
        }

        public async Task<PhotoUploadResult> UploadImageAsync(Stream fileStream, string fileName, string folder = "tutor_platform")
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                return new PhotoUploadResult { Success = false, ErrorMessage = "File ảnh không hợp lệ hoặc rỗng." };
            }

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                return new PhotoUploadResult
                {
                    Success = false,
                    ErrorMessage = uploadResult.Error.Message
                };
            }

            return new PhotoUploadResult
            {
                Success = true,
                Url = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString(),
                PublicId = uploadResult.PublicId
            };
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return false;

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
    }
}
