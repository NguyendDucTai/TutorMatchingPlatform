using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Infrastructure.Data.Configurations
{
    public class CreditRequestConfiguration : IEntityTypeConfiguration<CreditRequest>
    {
        public void Configure(EntityTypeBuilder<CreditRequest> builder)
        {
            builder.HasKey(cr => cr.Id);

            builder.Property(cr => cr.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(cr => cr.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(cr => cr.Note)
                .HasMaxLength(500);

            builder.HasOne(cr => cr.User)
                .WithMany(u => u.CreditRequests)
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
