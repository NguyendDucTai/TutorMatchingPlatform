using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TutorPlatform.Application.Common.Exceptions;
using TutorPlatform.Application.Contracts.Credits;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Credits.Queries.GetBalance
{
    public class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, WalletBalanceDto>
    {
        private readonly IUserRepository _userRepository;

        public GetBalanceQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<WalletBalanceDto> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException(nameof(User), request.UserId);
            }

            return new WalletBalanceDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                CreditBalance = user.CreditBalance
            };
        }
    }
}
