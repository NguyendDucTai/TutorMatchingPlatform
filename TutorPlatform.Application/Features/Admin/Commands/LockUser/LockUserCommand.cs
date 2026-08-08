using MediatR;
using TutorPlatform.Domain.Interfaces;

namespace TutorPlatform.Application.Features.Admin.Commands.LockUser
{
    public class LockUserCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
    }

    public class LockUserCommandHandler : IRequestHandler<LockUserCommand, bool>
    {
        private readonly IAdminRepository _adminRepository;

        public LockUserCommandHandler(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<bool> Handle(LockUserCommand request, CancellationToken cancellationToken)
        {
            return await _adminRepository.LockUserAsync(request.UserId);
        }
    }
}
