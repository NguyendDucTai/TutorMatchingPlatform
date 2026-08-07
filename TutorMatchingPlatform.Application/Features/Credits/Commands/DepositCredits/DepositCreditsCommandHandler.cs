using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorMatchingPlatform.Application.Common.Exceptions;
using TutorMatchingPlatform.Application.Common.Interfaces;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;
using TutorMatchingPlatform.Domain.Interfaces;

namespace TutorMatchingPlatform.Application.Features.Credits.Commands.DepositCredits
{
    public class DepositCreditsCommandHandler : IRequestHandler<DepositCreditsCommand, decimal>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICreditTransactionRepository _creditTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public DepositCreditsCommandHandler(
            IUserRepository userRepository,
            ICreditTransactionRepository creditTransactionRepository,
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _userRepository = userRepository;
            _creditTransactionRepository = creditTransactionRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<decimal> Handle(DepositCreditsCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException(nameof(User), request.UserId);
            }

            // Student role is checked at API level, but we adjust the credits here
            user.AdjustCredits(request.Amount);

            var tx = new CreditTransaction(
                request.UserId,
                request.Amount,
                CreditTransactionType.Credit,
                $"Deposited {request.Amount:N2} credits to wallet.",
                user.CreditBalance);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _userRepository.UpdateAsync(user);
                await _creditTransactionRepository.AddAsync(tx);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            await _publisher.Publish(new TutorMatchingPlatform.Application.Features.Notifications.Events.CreditChangedEvent
            {
                UserId = request.UserId,
                Amount = request.Amount,
                NewBalance = user.CreditBalance,
                Reason = "Deposit"
            }, cancellationToken);

            return user.CreditBalance;
        }
    }
}
