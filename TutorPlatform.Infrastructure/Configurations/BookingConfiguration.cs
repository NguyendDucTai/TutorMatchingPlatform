using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<BookingDataModel>
    {
        public void Configure(EntityTypeBuilder<BookingDataModel> builder)
        {
            builder.ToTable("Bookings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MeetingLink)
                   .HasMaxLength(500);

            builder.Property(x => x.Notes)
                   .HasMaxLength(1000);

            builder.Property(x => x.CancellationReason)
                   .HasMaxLength(500);

            builder.Property(x => x.CreditAmount)
                   .HasColumnType("decimal(18,2)");

            builder.HasIndex(x => x.TutorId);
            builder.HasIndex(x => x.StudentId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.ScheduledStartAt);

            builder.HasOne(x => x.Tutor)
                   .WithMany(x => x.BookingsAsTutor)
                   .HasForeignKey(x => x.TutorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Student)
                   .WithMany(x => x.BookingsAsStudent)
                   .HasForeignKey(x => x.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Subject)
                   .WithMany(x => x.Bookings)
                   .HasForeignKey(x => x.SubjectId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
