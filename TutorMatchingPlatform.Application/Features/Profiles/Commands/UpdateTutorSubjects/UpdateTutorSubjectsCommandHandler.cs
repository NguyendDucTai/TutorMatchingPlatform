using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorMatchingPlatform.Application.Common.Exceptions;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Interfaces;

namespace TutorMatchingPlatform.Application.Features.Profiles.Commands.UpdateTutorSubjects
{
    public class UpdateTutorSubjectsCommandHandler : IRequestHandler<UpdateTutorSubjectsCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISubjectRepository _subjectRepository;

        public UpdateTutorSubjectsCommandHandler(IUserRepository userRepository, ISubjectRepository subjectRepository)
        {
            _userRepository = userRepository;
            _subjectRepository = subjectRepository;
        }

        public async Task<bool> Handle(UpdateTutorSubjectsCommand request, CancellationToken cancellationToken)
        {
            var profile = await _userRepository.GetTutorProfileAsync(request.UserId);
            if (profile == null)
            {
                profile = new TutorProfile(request.UserId, null, null);
                profile.Approve(request.UserId);
                await _userRepository.AddTutorProfileAsync(profile);
            }

            // Verify all subjects exist
            var allSubjectIds = request.Subjects.Select(s => s.SubjectId).ToList();
            foreach(var subId in allSubjectIds)
            {
                var subject = await _subjectRepository.GetByIdAsync(subId);
                if (subject == null)
                {
                    throw new NotFoundException(nameof(Subject), subId);
                }
            }

            // Bulk update logic: clear and re-add.
            profile.ClearTutorSubjects();
            
            foreach (var s in request.Subjects)
            {
                var ts = new TutorSubject(profile.Id, s.SubjectId, (TutorMatchingPlatform.Domain.Enums.ProficiencyLevel)s.ProficiencyLevel, s.HourlyCredits);
                profile.AddTutorSubject(ts);
            }

            await _userRepository.UpdateTutorSubjectsAsync(request.UserId, profile.TutorSubjects);

            return true;
        }
    }
}
