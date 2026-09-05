using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TutorPlatform.Infrastructure.Persistence;

namespace TutorPlatform.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<TutorPlatform.Domain.Interfaces.IUserRepository, TutorPlatform.Infrastructure.Repositories.UserRepository>();
            services.AddScoped<TutorPlatform.Domain.Interfaces.ITutorSearchRepository, TutorPlatform.Infrastructure.Repositories.TutorSearchRepository>();
            services.AddScoped<TutorPlatform.Domain.Interfaces.ICreditTransactionRepository, TutorPlatform.Infrastructure.Repositories.CreditTransactionRepository>();
            services.AddScoped<TutorPlatform.Domain.Interfaces.ISubjectRepository, TutorPlatform.Infrastructure.Repositories.SubjectRepository>();
            services.AddScoped<TutorPlatform.Domain.Interfaces.IAvailabilityRepository, TutorPlatform.Infrastructure.Repositories.AvailabilityRepository>();
            services.AddScoped<TutorPlatform.Domain.Interfaces.IBookingRepository, TutorPlatform.Infrastructure.Repositories.BookingRepository>();
            services.AddScoped<TutorPlatform.Domain.Interfaces.ILearningGoalRepository, TutorPlatform.Infrastructure.Repositories.LearningGoalRepository>();
            services.AddScoped<TutorPlatform.Domain.Interfaces.ISessionRecordRepository, TutorPlatform.Infrastructure.Repositories.SessionRecordRepository>();
            services.AddScoped<TutorPlatform.Domain.Interfaces.IReviewRepository, TutorPlatform.Infrastructure.Repositories.ReviewRepository>();
            services.AddScoped<TutorPlatform.Domain.Interfaces.IAdminRepository, TutorPlatform.Infrastructure.Repositories.AdminRepository>();
            services.AddScoped<TutorPlatform.Domain.Interfaces.INotificationRepository, TutorPlatform.Infrastructure.Repositories.NotificationRepository>();
            services.AddScoped<TutorPlatform.Domain.Interfaces.IPasswordResetTokenRepository, TutorPlatform.Infrastructure.Repositories.PasswordResetTokenRepository>();
            services.AddScoped<TutorPlatform.Application.Common.Interfaces.ICreditService, TutorPlatform.Infrastructure.Services.CreditService>();
            services.AddScoped<TutorPlatform.Application.Common.Interfaces.IUnitOfWork, TutorPlatform.Infrastructure.Services.UnitOfWork>();
            services.AddSingleton<TutorPlatform.Application.Common.Interfaces.IJwtTokenGenerator, TutorPlatform.Infrastructure.Services.JwtTokenGenerator>();
            services.AddSingleton<TutorPlatform.Application.Common.Interfaces.IPasswordHasher, TutorPlatform.Infrastructure.Services.BCryptPasswordHasher>();

            services.Configure<TutorPlatform.Infrastructure.Configurations.EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddTransient<TutorPlatform.Application.Common.Interfaces.IEmailService, TutorPlatform.Infrastructure.Services.EmailService>();

            services.Configure<TutorPlatform.Infrastructure.Configurations.CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.AddScoped<TutorPlatform.Application.Common.Interfaces.IPhotoService, TutorPlatform.Infrastructure.Services.CloudinaryPhotoService>();

            services.AddHostedService<TutorPlatform.Infrastructure.Services.OverdueClassNotifierService>();

            return services;
        }
    }
}
