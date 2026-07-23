using System.Collections.Generic;
using MediatR;

namespace TutorMatchingPlatform.Application.Credits.Queries.GetPendingCreditRequests
{
    public class GetPendingCreditRequestsQuery : IRequest<List<CreditRequestDto>>
    {
    }
}
