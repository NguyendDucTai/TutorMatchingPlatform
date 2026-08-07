using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using TutorMatchingPlatform.Application.Contracts.Admin;
using TutorMatchingPlatform.Domain.Interfaces;

namespace TutorMatchingPlatform.Application.Features.Admin.Queries.GetAdminDashboard
{
    public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
    {
        private readonly IAdminRepository _adminRepository;

        public GetAdminDashboardQueryHandler(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
        {
            var stats = await _adminRepository.GetDashboardStatsAsync();
            return stats.Adapt<AdminDashboardDto>();
        }
    }
}
