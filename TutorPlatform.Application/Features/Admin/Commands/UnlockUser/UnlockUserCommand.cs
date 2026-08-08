using MediatR;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Admin.Commands.UnlockUser
{
    public class UnlockUserCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
    }

    public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand, bool>
    {
        private readonly IAdminRepository _adminRepository;

        public UnlockUserCommandHandler(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<bool> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
        {
            return await _adminRepository.UnlockUserAsync(request.UserId);
        }
    }
}
