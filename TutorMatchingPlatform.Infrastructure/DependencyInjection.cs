using Microsoft.Extensions.DependencyInjection;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Interfaces.Authentication;
using TutorMatchingPlatform.Infrastructure.Authentication;
using TutorMatchingPlatform.Infrastructure.Data;
using TutorMatchingPlatform.Infrastructure.Services;

namespace TutorMatchingPlatform.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // The DbContext is registered in Program.cs (API) to allow connection string setup there,
            // but we bind the interface to the context.
            services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<TutorMatchingPlatformDbContext>());

            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IFileService, LocalFileService>();

            return services;
        }
    }
}
