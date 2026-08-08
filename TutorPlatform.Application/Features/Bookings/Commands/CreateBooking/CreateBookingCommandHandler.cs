using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly ICreditService _creditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public CreateBookingCommandHandler(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IAvailabilityRepository availabilityRepository,
            ICreditService creditService,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _availabilityRepository = availabilityRepository;
            _creditService = creditService;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var student = await _userRepository.GetByIdAsync(request.StudentId);
            if (student == null) return CreateBookingResult.Fail("Student not found.");

            var tutorUser = await _userRepository.GetByIdAsync(request.TutorId);
            if (tutorUser == null) return CreateBookingResult.Fail("Tutor not found.");
            if (!tutorUser.IsActive) return CreateBookingResult.Fail("Tài khoản gia sư đã bị sa thải hoặc vô hiệu hóa.");

            var tutorProfile = await _userRepository.GetTutorProfileAsync(request.TutorId);
            if (tutorProfile == null) return CreateBookingResult.Fail("Tutor profile not found.");
            if (!tutorProfile.IsApproved) return CreateBookingResult.Fail("Hồ sơ gia sư chưa được Admin phê duyệt hoặc đã bị từ chối.");

            var tutorSubject = tutorProfile.TutorSubjects.FirstOrDefault(ts => ts.SubjectId == request.SubjectId);
            if (tutorSubject == null) return CreateBookingResult.Fail("Tutor does not teach this subject.");

            // Convert incoming UTC/ISO datetime to Vietnam local time (UTC+7) for consistent validation and database storage
            var reqStart = request.ScheduledStartAt.ToUniversalTime().AddHours(7);
            var reqEnd = request.ScheduledEndAt.ToUniversalTime().AddHours(7);
            var reqTimeStart = reqStart.TimeOfDay;
            var reqTimeEnd = reqEnd.TimeOfDay;

            // Check availability bounds logic:
            var availabilities = await _availabilityRepository.GetByUserIdAsync(request.TutorId);
            bool isTimeSlotValid = false;

            foreach (var av in availabilities)
            {
                if (av.IsRecurring && av.DayOfWeek.HasValue && (int)av.DayOfWeek.Value == (int)reqStart.DayOfWeek)
                {
                    if (reqTimeStart >= av.StartTime && reqTimeEnd <= av.EndTime) isTimeSlotValid = true;
                }
                else if (!av.IsRecurring && av.SpecificDate.HasValue && av.SpecificDate.Value.Date == reqStart.Date)
                {
                    if (reqTimeStart >= av.StartTime && reqTimeEnd <= av.EndTime) isTimeSlotValid = true;
                }
            }

            if (!isTimeSlotValid)
            {
                return CreateBookingResult.Fail("Tutor is not available at this time slot.", isConflict: true);
            }

            // Check if tutor already has a CONFIRMED booking that overlaps the requested time slot.
            // Pending bookings are NOT considered conflicts (allows group class registrations for the same slot).
            // Overlap condition: existing.Start < reqEnd AND existing.End > reqStart
            var hasConfirmedOverlap = await _bookingRepository.HasConfirmedOverlappingBookingAsync(request.TutorId, reqStart, reqEnd);
            if (hasConfirmedOverlap)
            {
                return CreateBookingResult.Fail("Gia sư hiện đang có lịch dạy trong khoảng thời gian này. Vui lòng chọn gia sư khác hoặc đợi đến khi buổi học kết thúc.", isConflict: true);
            }

            // Check if student already has a CONFIRMED booking that overlaps the requested time slot.
            var hasStudentOverlap = await _bookingRepository.HasStudentConfirmedOverlappingBookingAsync(request.StudentId, reqStart, reqEnd);
            if (hasStudentOverlap)
            {
                return CreateBookingResult.Fail("Bạn đã có lịch học khác trùng thời gian trong khoảng thời gian này. Vui lòng kiểm tra lại lịch học của mình.", isConflict: true);
            }

            // Check if student already has a PENDING or CONFIRMED booking for the same subject and time slot (prevents registering with multiple tutors for same slot & subject)
            var hasActiveBooking = await _bookingRepository.HasStudentActiveBookingForSubjectAsync(request.StudentId, reqStart, reqEnd, request.SubjectId);
            if (hasActiveBooking)
            {
                return CreateBookingResult.Fail("Bạn đã đăng ký học môn này vào khung giờ trùng lặp và yêu cầu đang chờ gia sư xác nhận hoặc đang hoạt động. Vui lòng không đăng ký trùng môn và giờ.", isConflict: true);
            }

            // Calculate cost
            var durationHours = (decimal)(reqEnd - reqStart).TotalHours;
            var cost = tutorSubject.HourlyCredits * durationHours;

            var bookingId = Guid.NewGuid();
            var booking = new Booking(bookingId, request.TutorId, request.StudentId, request.SubjectId, reqStart, reqEnd, cost, request.Notes);

            // Bug #5: Wrap debit + save in a single transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _bookingRepository.AddAsync(booking);
                await _creditService.DebitAsync(request.StudentId, cost, $"Booking holding for {reqStart.ToString("g")}", bookingId);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            // Publish Event to trigger Notifications (Safe side-effect)
            await _publisher.Publish(new TutorPlatform.Application.Features.Notifications.Events.BookingCreatedEvent
            {
                Booking = booking,
                StudentName = student.FullName
            }, cancellationToken);

            return CreateBookingResult.Ok(bookingId);
        }
    }
}
