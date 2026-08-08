using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class SessionRecordConfiguration : IEntityTypeConfiguration<SessionRecordDataModel>
    {
        public void Configure(EntityTypeBuilder<SessionRecordDataModel> builder)
        {
            builder.ToTable("SessionRecords");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Score)
                   .HasColumnType("decimal(5,2)");

            builder.Property(x => x.CompletionPercentage)
                   .HasDefaultValue(0);

            builder.Property(x => x.TutorNotes)
                   .HasMaxLength(2000);

            builder.Property(x => x.Strengths)
                   .HasMaxLength(1000);

            builder.Property(x => x.AreasForImprovement)
                   .HasMaxLength(1000);

            builder.HasIndex(x => x.BookingId).IsUnique();

            builder.HasOne(x => x.Booking)
                   .WithOne(x => x.SessionRecord)
                   .HasForeignKey<SessionRecordDataModel>(x => x.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
