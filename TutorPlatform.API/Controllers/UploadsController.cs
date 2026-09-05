using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorPlatform.API.Common;
using TutorPlatform.Application.Common.Interfaces;

namespace TutorPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadsController : ControllerBase
    {
        private readonly IPhotoService _photoService;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public UploadsController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        [HttpPost("image")]
        [ProducesResponseType(typeof(ApiResponse<UploadImageResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] string? folder = "tutor_platform")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<object>.Error(400, "Vui lòng chọn một file ảnh hợp lệ."));
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest(ApiResponse<object>.Error(400, "Dung lượng file ảnh không được vượt quá 5MB."));
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(ApiResponse<object>.Error(400, "Định dạng ảnh không được hỗ trợ. Vui lòng chọn file .jpg, .jpeg, .png hoặc .webp."));
            }

            var cleanFolder = string.IsNullOrWhiteSpace(folder) ? "tutor_platform" : folder.Trim();

            using var stream = file.OpenReadStream();
            var result = await _photoService.UploadImageAsync(stream, file.FileName, cleanFolder);

            if (!result.Success)
            {
                return BadRequest(ApiResponse<object>.Error(400, result.ErrorMessage ?? "Tải ảnh lên Cloudinary thất bại."));
            }

            return Ok(ApiResponse<UploadImageResponse>.Ok(new UploadImageResponse
            {
                Url = result.Url!,
                PublicId = result.PublicId!
            }));
        }
    }

    public class UploadImageResponse
    {
        public string Url { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
    }
}
