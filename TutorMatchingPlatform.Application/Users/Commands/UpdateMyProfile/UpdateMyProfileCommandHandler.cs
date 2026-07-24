using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, UpdateMyProfileResult>
    {
        private readonly IAppDbContext _context;
        private readonly IFileService _fileService;

        public UpdateMyProfileCommandHandler(IAppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<UpdateMyProfileResult> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.TutorProfile)
                .Include(u => u.StudentProfile)
                .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return new UpdateMyProfileResult { Success = false, Message = "User not found." };
            }

            user.FullName = request.FullName;

            if (request.AvatarFile != null)
            {
                var avatarUrl = await _fileService.UploadFileAsync(
                    request.AvatarFile, 
                    "avatars", 
                    new[] { ".jpg", ".jpeg", ".png" }, 
                    5);
                user.AvatarUrl = avatarUrl;
            }


            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateMyProfileResult 
            { 
                Success = true, 
                Message = "Profile updated successfully.",
                AvatarUrl = user.AvatarUrl
            };
        }
    }
}
