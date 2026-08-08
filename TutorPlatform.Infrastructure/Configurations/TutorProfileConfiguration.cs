using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class TutorProfileConfiguration : IEntityTypeConfiguration<TutorProfileDataModel>
    {
        public void Configure(EntityTypeBuilder<TutorProfileDataModel> builder)
        {
            builder.ToTable("TutorProfiles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Bio)
                   .HasMaxLength(2000);

            builder.Property(x => x.Qualifications)
                   .HasMaxLength(1000);

            builder.Property(x => x.AverageRating)
                   .HasColumnType("decimal(3,2)")
                   .HasDefaultValue(0);

            builder.HasIndex(x => x.UserId).IsUnique();
            builder.HasIndex(x => x.IsApproved);

            builder.HasOne(x => x.User)
                   .WithOne(x => x.TutorProfile)
                   .HasForeignKey<TutorProfileDataModel>(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
