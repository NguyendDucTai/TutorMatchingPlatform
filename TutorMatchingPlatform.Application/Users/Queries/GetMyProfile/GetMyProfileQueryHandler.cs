using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Users.DTOs;

namespace TutorMatchingPlatform.Application.Users.Queries.GetMyProfile
{
    public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserProfileDto>
    {
        private readonly IAppDbContext _context;

        public GetMyProfileQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.StudentProfile)
                .Include(u => u.TutorProfile)
                .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null) return null!;

            var dto = new UserProfileDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                AvatarUrl = user.AvatarUrl,
                IsSuspended = user.IsSuspended
            };

            if (user.TutorProfile != null)
            {
                dto.Bio = user.TutorProfile.Bio;
                dto.Qualifications = user.TutorProfile.Qualifications;
                dto.SubjectsJson = user.TutorProfile.SubjectsJson;
                dto.FreeSchedulesJson = user.TutorProfile.FreeSchedulesJson;
                dto.TimezoneOffset = user.TutorProfile.TimezoneOffset;
                dto.TutorStatus = user.TutorProfile.Status.ToString();
            }

            if (user.StudentProfile != null)
            {
                dto.StudyGoals = user.StudentProfile.StudyGoals;
                dto.TargetSubjectsJson = user.StudentProfile.TargetSubjectsJson;
            }

            return dto;
        }
    }
}
