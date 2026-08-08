using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class AvailabilityConfiguration : IEntityTypeConfiguration<AvailabilityDataModel>
    {
        public void Configure(EntityTypeBuilder<AvailabilityDataModel> builder)
        {
            builder.ToTable("Availabilities");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SpecificDate).HasColumnType("date");

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => new { x.UserId, x.DayOfWeek });

            builder.HasOne(x => x.User)
                   .WithMany(x => x.Availabilities)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
