using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Contracts.Credits;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Credits.Queries.GetCreditTransactions
{
    public class GetCreditTransactionsQueryHandler : IRequestHandler<GetCreditTransactionsQuery, PagedResult<CreditTransactionDto>>
    {
        private readonly ICreditTransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;

        public GetCreditTransactionsQueryHandler(
            ICreditTransactionRepository transactionRepository,
            IUserRepository userRepository)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResult<CreditTransactionDto>> Handle(GetCreditTransactionsQuery request, CancellationToken cancellationToken)
        {
            var queryUserId = request.UserId;

            if (request.TargetUserId.HasValue && request.TargetUserId.Value != request.UserId)
            {
                if (request.RequestorRole != "Admin")
                {
                    throw new BadRequestException("Only administrators can view other users' transaction history.");
                }
                queryUserId = request.TargetUserId.Value;

                // Validate target user exists
                var targetUser = await _userRepository.GetByIdAsync(queryUserId);
                if (targetUser == null)
                {
                    throw new NotFoundException("User", queryUserId);
                }
            }

            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var pagedResult = await _transactionRepository.GetByUserIdAsync(queryUserId, pageNumber, pageSize);

            var dtos = pagedResult.Items.Select(tx => new CreditTransactionDto
            {
                Id = tx.Id,
                Amount = tx.Amount,
                Type = MapToTypeString(tx.Type),
                Description = tx.Description,
                BookingId = tx.BookingId,
                BalanceAfter = tx.BalanceAfter,
                CreatedAt = tx.CreatedAt
            }).ToList();

            return new PagedResult<CreditTransactionDto>(dtos, pagedResult.TotalCount);
        }

        private string MapToTypeString(CreditTransactionType type)
        {
            return type switch
            {
                CreditTransactionType.Credit => "Deposit",
                CreditTransactionType.Debit => "Booking_Hold",
                CreditTransactionType.Refund => "Refund",
                CreditTransactionType.Transfer => "Tutor_Payout",
                _ => "Unknown"
            };
        }
    }
}
