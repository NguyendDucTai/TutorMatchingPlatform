using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Interfaces;

namespace TutorMatchingPlatform.Application.Features.Bookings.Queries.GetMyBookings
{
    public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, PagedResult<BookingDto>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReviewRepository _reviewRepository;

        public GetMyBookingsQueryHandler(
            IBookingRepository bookingRepository, 
            IUserRepository userRepository, 
            ISubjectRepository subjectRepository,
            IReviewRepository reviewRepository)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _subjectRepository = subjectRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<PagedResult<BookingDto>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
        {
            var pagedData = await _bookingRepository.GetPagedAsync(request.UserId, request.Role, request.PageNumber, request.PageSize);

            var items = new System.Collections.Generic.List<BookingDto>();
            foreach (var b in pagedData.Items)
            {
                var student = await _userRepository.GetByIdAsync(b.StudentId);
                var tutor = await _userRepository.GetByIdAsync(b.TutorId);
                var subject = await _subjectRepository.GetByIdAsync(b.SubjectId);

                var studentReview = await _reviewRepository.GetByBookingAndTypeAsync(b.Id, Domain.Enums.ReviewType.StudentToTutor);
                var tutorReview = await _reviewRepository.GetByBookingAndTypeAsync(b.Id, Domain.Enums.ReviewType.TutorToStudent);

                items.Add(new BookingDto
                {
                    Id = b.Id,
                    TutorId = b.TutorId,
                    StudentId = b.StudentId,
                    SubjectId = b.SubjectId,
                    ScheduledStartAt = b.ScheduledStartAt,
                    ScheduledEndAt = b.ScheduledEndAt,
                    Status = (int)b.Status,
                    MeetingLink = b.MeetingLink,
                    CreditAmount = b.CreditAmount,
                    StudentName = student?.FullName ?? "Unknown",
                    TutorName = tutor?.FullName ?? "Unknown",
                    SubjectName = subject?.Name ?? "Unknown",
                    IsStudentReviewed = studentReview != null,
                    IsTutorReviewed = tutorReview != null,
                    StudentRating = studentReview?.Rating,
                    StudentComment = studentReview?.Comment,
                    TutorRating = tutorReview?.Rating,
                    TutorComment = tutorReview?.Comment
                });
            }

            return new PagedResult<BookingDto>(items, pagedData.TotalCount);
        }
    }
}
