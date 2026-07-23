using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace TutorMatchingPlatform.Application.Tutors.Commands.UpdateTutorAvailability
{
    public class UpdateTutorAvailabilityCommandHandler : IRequestHandler<UpdateTutorAvailabilityCommand, bool>
    {
        private readonly IAppDbContext _context;

        public UpdateTutorAvailabilityCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateTutorAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.TutorProfile)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null || user.Role != UserRole.Tutor || user.TutorProfile == null)
            {
                return false;
            }

            user.TutorProfile.FreeSchedulesJson = request.FreeSchedulesJson;
            user.TutorProfile.TimezoneOffset = request.TimezoneOffset;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
