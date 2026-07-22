using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces.Authentication;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(TutorMatchingPlatformDbContext context, IPasswordHasher passwordHasher)
        {
            await context.Database.MigrateAsync();

            var adminExists = await context.Users.AnyAsync(u => u.Role == UserRole.Administrator);

            if (!adminExists)
            {
                var adminUser = new User
                {
                    FullName = "System Administrator",
                    Email = "admin@tutormatching.com",
                    PasswordHash = passwordHasher.HashPassword("Admin@123"),
                    Role = UserRole.Administrator,
                    IsSuspended = false,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }
}
