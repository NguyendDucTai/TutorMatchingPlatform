using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using TutorPlatform.Application.Contracts.Admin;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Admin.Queries.GetPendingTutors
{
    public class GetPendingTutorsQueryHandler : IRequestHandler<GetPendingTutorsQuery, PagedResult<PendingTutorDto>>
    {
        private readonly IAdminRepository _adminRepository;

        public GetPendingTutorsQueryHandler(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<PagedResult<PendingTutorDto>> Handle(GetPendingTutorsQuery request, CancellationToken cancellationToken)
        {
            var pagedResult = await _adminRepository.GetPendingTutorsAsync(request.PageNumber, request.PageSize);
            var mappedItems = pagedResult.Items.Select(x => x.Adapt<PendingTutorDto>()).ToList();
            return new PagedResult<PendingTutorDto>(mappedItems.AsReadOnly(), pagedResult.TotalCount);
        }
    }
}
