using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Application.Interfaces.Authentication
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
