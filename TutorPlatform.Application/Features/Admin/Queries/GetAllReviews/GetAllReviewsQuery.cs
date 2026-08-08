using MediatR;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Admin.Queries.GetAllReviews
{
    public class GetAllReviewsQuery : IRequest<PagedResult<AdminReviewResult>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public int? ReviewType { get; set; }
        public int? Rating { get; set; }
    }

    public class GetAllReviewsQueryHandler : IRequestHandler<GetAllReviewsQuery, PagedResult<AdminReviewResult>>
    {
        private readonly IAdminRepository _adminRepository;

        public GetAllReviewsQueryHandler(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<PagedResult<AdminReviewResult>> Handle(GetAllReviewsQuery request, CancellationToken cancellationToken)
        {
            return await _adminRepository.GetAllReviewsAsync(request.PageNumber, request.PageSize, request.Search, request.ReviewType, request.Rating);
        }
    }
}
