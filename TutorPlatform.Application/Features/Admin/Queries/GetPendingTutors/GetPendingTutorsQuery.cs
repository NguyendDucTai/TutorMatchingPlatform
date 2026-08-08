using MediatR;
using TutorPlatform.Application.Contracts.Admin;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.Application.Features.Admin.Queries.GetPendingTutors
{
    public class GetPendingTutorsQuery : IRequest<PagedResult<PendingTutorDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
