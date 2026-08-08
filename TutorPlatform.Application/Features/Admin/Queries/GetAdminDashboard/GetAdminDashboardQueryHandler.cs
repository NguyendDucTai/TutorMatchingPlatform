using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using TutorPlatform.Application.Contracts.Admin;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Admin.Queries.GetAdminDashboard
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
