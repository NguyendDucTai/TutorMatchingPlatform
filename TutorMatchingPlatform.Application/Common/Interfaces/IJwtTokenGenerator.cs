using System;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
    }
}
