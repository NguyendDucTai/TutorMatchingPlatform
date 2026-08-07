using MediatR;
using TutorMatchingPlatform.Application.Contracts.Admin;

namespace TutorMatchingPlatform.Application.Features.Admin.Queries.GetAdminDashboard
{
    public class GetAdminDashboardQuery : IRequest<AdminDashboardDto>
    {
    }
}
