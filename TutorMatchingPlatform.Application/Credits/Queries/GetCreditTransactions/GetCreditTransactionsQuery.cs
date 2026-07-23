using System.Collections.Generic;
using MediatR;

namespace TutorMatchingPlatform.Application.Credits.Queries.GetCreditTransactions
{
    public class GetCreditTransactionsQuery : IRequest<List<CreditTransactionDto>>
    {
        public int UserId { get; set; }
    }
}
