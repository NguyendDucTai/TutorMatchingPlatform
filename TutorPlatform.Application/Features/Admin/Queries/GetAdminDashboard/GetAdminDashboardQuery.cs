using MediatR;
using TutorPlatform.Application.Contracts.Admin;

namespace TutorPlatform.Application.Features.Admin.Queries.GetAdminDashboard
{
    public class GetAdminDashboardQuery : IRequest<AdminDashboardDto>
    {
    }
}
