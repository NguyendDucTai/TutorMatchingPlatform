using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TutorMatchingPlatform.Infrastructure.Data;

namespace TutorMatchingPlatform.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = "IntegrationTestDb_" + Guid.NewGuid().ToString("N");
        private readonly InMemoryDatabaseRoot _databaseRoot = new InMemoryDatabaseRoot();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "JwtSettings:Secret", "SuperSecretKeyForIntegrationTesting123456789!" },
                    { "JwtSettings:Issuer", "TutorMatchingPlatform" },
                    { "JwtSettings:Audience", "TutorMatchingPlatform" },
                    { "JwtSettings:ExpiryMinutes", "60" }
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove all existing DbContext and EF Core registrations
                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<TutorMatchingPlatformDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(TutorMatchingPlatformDbContext) ||
                    (d.ServiceType.Namespace != null && d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore"))
                ).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Add In-Memory DbContext with fixed database name per factory instance
                services.AddDbContext<TutorMatchingPlatformDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName, _databaseRoot);
                });
            });
        }
    }
}
