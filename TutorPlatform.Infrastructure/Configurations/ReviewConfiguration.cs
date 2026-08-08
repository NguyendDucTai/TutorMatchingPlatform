using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<ReviewDataModel>
    {
        public void Configure(EntityTypeBuilder<ReviewDataModel> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Comment)
                   .HasMaxLength(2000);

            builder.HasIndex(x => new { x.BookingId, x.ReviewType }).IsUnique();
            builder.HasIndex(x => x.ReviewerId);
            builder.HasIndex(x => x.RevieweeId);

            builder.HasOne(x => x.Booking)
                   .WithMany(x => x.Reviews)
                   .HasForeignKey(x => x.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Reviewer)
                   .WithMany()
                   .HasForeignKey(x => x.ReviewerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Reviewee)
                   .WithMany()
                   .HasForeignKey(x => x.RevieweeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
