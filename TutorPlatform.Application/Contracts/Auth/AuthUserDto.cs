using System;

namespace TutorPlatform.Application.Contracts.Auth
{
    public class AuthUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int Role { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
