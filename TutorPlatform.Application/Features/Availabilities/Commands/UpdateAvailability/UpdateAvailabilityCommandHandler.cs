using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Availabilities.Commands.UpdateAvailability
{
    public class UpdateAvailabilityCommandHandler : IRequestHandler<UpdateAvailabilityCommand, bool>
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAdminRepository _adminRepository;

        public UpdateAvailabilityCommandHandler(
            IAvailabilityRepository availabilityRepository, 
            IUserRepository userRepository,
            IAdminRepository adminRepository)
        {
            _availabilityRepository = availabilityRepository;
            _userRepository = userRepository;
            _adminRepository = adminRepository;
        }

        public async Task<bool> Handle(UpdateAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var tutor = await _userRepository.GetTutorProfileAsync(request.TutorId);
            if (tutor == null)
            {
                throw new NotFoundException(nameof(TutorProfile), request.TutorId);
            }

            var newDomainAvailabilities = new List<Availability>();
            foreach (var dto in request.Availabilities)
            {
                if (!TimeSpan.TryParse(dto.StartTime, out var startTime) || !TimeSpan.TryParse(dto.EndTime, out var endTime))
                {
                    throw new BadRequestException("Invalid time format. Use HH:mm:ss.");
                }

                if (dto.IsRecurring)
                {
                    if (!dto.DayOfWeek.HasValue) throw new BadRequestException("DayOfWeek is required for recurring availability.");
                    newDomainAvailabilities.Add(new Availability(request.TutorId, (DayOfWeek)dto.DayOfWeek.Value, startTime, endTime));
                }
                else
                {
                    if (!dto.SpecificDate.HasValue) throw new BadRequestException("SpecificDate is required for non-recurring availability.");
                    newDomainAvailabilities.Add(new Availability(request.TutorId, dto.SpecificDate.Value, startTime, endTime));
                }
            }

            // Conflict Check
            var hasConflict = await _availabilityRepository.HasConflictingBookingsAsync(request.TutorId, newDomainAvailabilities);
            if (hasConflict)
            {
                throw new ConflictException("Cannot update availability because there are conflicting active bookings.");
            }

            // Bulk Replace
            await _availabilityRepository.BulkReplaceAsync(request.TutorId, newDomainAvailabilities);

            // Check if profile details + availability are complete and notify admin
            await _adminRepository.CheckAndNotifyAdminOnTutorProfileCompletionAsync(request.TutorId);

            return true;
        }
    }
}
