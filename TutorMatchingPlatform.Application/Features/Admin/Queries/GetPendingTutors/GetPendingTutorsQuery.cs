using MediatR;
using TutorMatchingPlatform.Application.Contracts.Admin;
using TutorMatchingPlatform.Domain.Common;

namespace TutorMatchingPlatform.Application.Features.Admin.Queries.GetPendingTutors
{
    public class GetPendingTutorsQuery : IRequest<PagedResult<PendingTutorDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
