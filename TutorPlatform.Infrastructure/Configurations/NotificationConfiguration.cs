using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<NotificationDataModel>
    {
        public void Configure(EntityTypeBuilder<NotificationDataModel> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Message)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(x => x.RelatedEntityType)
                   .HasMaxLength(50);

            builder.Property(x => x.IsRead)
                   .HasDefaultValue(false);

            builder.HasIndex(x => new { x.UserId, x.IsRead });
            builder.HasIndex(x => x.CreatedAt);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.Notifications)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
