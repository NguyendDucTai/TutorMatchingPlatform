using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserDataModel>
    {
        public void Configure(EntityTypeBuilder<UserDataModel> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(x => x.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(512);

            builder.Property(x => x.FullName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Phone)
                   .HasMaxLength(20);

            builder.Property(x => x.AvatarUrl)
                   .HasMaxLength(500);

            builder.Property(x => x.CreditBalance)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            builder.Property(x => x.RefreshToken)
                   .HasMaxLength(512);

            builder.HasIndex(x => x.Email).IsUnique();

            // Relationships will be configured in dependent entities or here
        }
    }
}
