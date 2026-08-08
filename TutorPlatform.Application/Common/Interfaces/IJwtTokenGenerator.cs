using System;
using TutorPlatform.Domain.Entities;

namespace TutorPlatform.Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
    }
}
