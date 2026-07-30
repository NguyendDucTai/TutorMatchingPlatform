using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Profiles.Commands.UpdateTutorSubjects
{
    public class UpdateTutorSubjectsCommandHandler : IRequestHandler<UpdateTutorSubjectsCommand, bool>
    {
        private readonly IAppDbContext _context;

        public UpdateTutorSubjectsCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateTutorSubjectsCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.TutorProfile)
                .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null || user.Role != UserRole.Tutor || user.TutorProfile == null)
            {
                return false;
            }

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var existingSubjects = JsonSerializer.Deserialize<List<SubjectRateDto>>(
                user.TutorProfile.SubjectsJson ?? "[]",
                serializerOptions) ?? new List<SubjectRateDto>();

            var subjectsChanged = !existingSubjects
                .OrderBy(subject => subject.SubjectId)
                .Select(subject => (subject.SubjectId, subject.Rate))
                .SequenceEqual(
                    request.Subjects
                        .OrderBy(subject => subject.SubjectId)
                        .Select(subject => (subject.SubjectId, subject.Rate)));

            if (subjectsChanged)
            {
                user.TutorProfile.SubjectsJson = JsonSerializer.Serialize(request.Subjects);

                // Only changed teaching subjects/rates need another admin review.
                if (user.TutorProfile.Status == ProfileStatus.Approved)
                {
                    user.TutorProfile.Status = ProfileStatus.Pending;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
