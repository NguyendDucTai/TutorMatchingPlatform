using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TutorMatchingPlatform.Infrastructure.Persistence;

namespace TutorMatchingPlatform.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<TutorMatchingPlatform.Domain.Interfaces.IUserRepository, TutorMatchingPlatform.Infrastructure.Repositories.UserRepository>();
            services.AddScoped<TutorMatchingPlatform.Domain.Interfaces.ITutorSearchRepository, TutorMatchingPlatform.Infrastructure.Repositories.TutorSearchRepository>();
            services.AddScoped<TutorMatchingPlatform.Domain.Interfaces.ICreditTransactionRepository, TutorMatchingPlatform.Infrastructure.Repositories.CreditTransactionRepository>();
            services.AddScoped<TutorMatchingPlatform.Domain.Interfaces.ISubjectRepository, TutorMatchingPlatform.Infrastructure.Repositories.SubjectRepository>();
            services.AddScoped<TutorMatchingPlatform.Domain.Interfaces.IAvailabilityRepository, TutorMatchingPlatform.Infrastructure.Repositories.AvailabilityRepository>();
            services.AddScoped<TutorMatchingPlatform.Domain.Interfaces.IBookingRepository, TutorMatchingPlatform.Infrastructure.Repositories.BookingRepository>();
            services.AddScoped<TutorMatchingPlatform.Domain.Interfaces.ILearningGoalRepository, TutorMatchingPlatform.Infrastructure.Repositories.LearningGoalRepository>();
            services.AddScoped<TutorMatchingPlatform.Domain.Interfaces.ISessionRecordRepository, TutorMatchingPlatform.Infrastructure.Repositories.SessionRecordRepository>();
            services.AddScoped<TutorMatchingPlatform.Domain.Interfaces.IReviewRepository, TutorMatchingPlatform.Infrastructure.Repositories.ReviewRepository>();
            services.AddScoped<TutorMatchingPlatform.Domain.Interfaces.IAdminRepository, TutorMatchingPlatform.Infrastructure.Repositories.AdminRepository>();
            services.AddScoped<TutorMatchingPlatform.Domain.Interfaces.INotificationRepository, TutorMatchingPlatform.Infrastructure.Repositories.NotificationRepository>();
            services.AddScoped<TutorMatchingPlatform.Application.Common.Interfaces.ICreditService, TutorMatchingPlatform.Infrastructure.Services.CreditService>();
            services.AddScoped<TutorMatchingPlatform.Application.Common.Interfaces.IUnitOfWork, TutorMatchingPlatform.Infrastructure.Services.UnitOfWork>();
            services.AddSingleton<TutorMatchingPlatform.Application.Common.Interfaces.IJwtTokenGenerator, TutorMatchingPlatform.Infrastructure.Services.JwtTokenGenerator>();
            services.AddSingleton<TutorMatchingPlatform.Application.Common.Interfaces.IPasswordHasher, TutorMatchingPlatform.Infrastructure.Services.BCryptPasswordHasher>();

            return services;
        }
    }
}
