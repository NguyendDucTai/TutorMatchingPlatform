using System;
using MediatR;
using TutorMatchingPlatform.Application.Contracts.Credits;
using TutorMatchingPlatform.Domain.Common;

namespace TutorMatchingPlatform.Application.Features.Credits.Queries.GetCreditTransactions
{
    public class GetCreditTransactionsQuery : IRequest<PagedResult<CreditTransactionDto>>
    {
        public Guid UserId { get; set; }
        public Guid? TargetUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
