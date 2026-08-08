using MediatR;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Admin.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<PagedResult<AdminUserResult>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public int? Role { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<AdminUserResult>>
    {
        private readonly IAdminRepository _adminRepository;

        public GetAllUsersQueryHandler(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<PagedResult<AdminUserResult>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return await _adminRepository.GetAllUsersAsync(request.PageNumber, request.PageSize, request.Search, request.Role, request.IsActive);
        }
    }
}
