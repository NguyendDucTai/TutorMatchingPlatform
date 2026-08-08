using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Contracts.Profiles;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Profiles.Queries.GetMyProfile
{
    public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, MyProfileResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISubjectRepository _subjectRepository;

        public GetMyProfileQueryHandler(IUserRepository userRepository, ISubjectRepository subjectRepository)
        {
            _userRepository = userRepository;
            _subjectRepository = subjectRepository;
        }

        public async Task<MyProfileResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.User), request.UserId);
            }

            var response = new MyProfileResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = (int)user.Role,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl
            };

            if (user.Role == UserRole.Tutor)
            {
                var tutorProfile = await _userRepository.GetTutorProfileAsync(user.Id);
                if (tutorProfile == null)
                {
                    tutorProfile = new Domain.Entities.TutorProfile(user.Id, null, null);
                    await _userRepository.AddTutorProfileAsync(tutorProfile);
                }

                // Fetch all subjects to map names efficiently
                var allSubjects = await _subjectRepository.GetAllActiveAsync();
                var subjectDict = allSubjects.ToDictionary(s => s.Id, s => s.Name);

                response.TutorProfile = new TutorProfileDto
                {
                    Bio = tutorProfile.Bio,
                    Qualifications = tutorProfile.Qualifications,
                    IsApproved = tutorProfile.IsApproved,
                    ApprovalStatus = tutorProfile.ApprovalStatus,
                    DefaultMeetingLink = tutorProfile.DefaultMeetingLink,
                    AverageRating = tutorProfile.AverageRating,
                    TotalSessions = tutorProfile.TotalSessions,
                    Subjects = tutorProfile.TutorSubjects.Select(ts => new TutorSubjectDto
                    {
                        SubjectId = ts.SubjectId,
                        SubjectName = subjectDict.ContainsKey(ts.SubjectId) ? subjectDict[ts.SubjectId] : string.Empty,
                        ProficiencyLevel = (int)ts.ProficiencyLevel,
                        HourlyCredits = ts.HourlyCredits
                    }).ToList()
                };
            }
            else if (user.Role == UserRole.Student)
            {
                var studentProfile = await _userRepository.GetStudentProfileAsync(user.Id);
                if (studentProfile != null)
                {
                    response.StudentProfile = new StudentProfileDto
                    {
                        GradeLevel = studentProfile.GradeLevel,
                        LearningPreferences = studentProfile.LearningPreferences
                    };
                }
            }

            return response;
        }
    }
}
