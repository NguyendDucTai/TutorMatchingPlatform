using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Infrastructure.Data.Configurations
{
    public class CreditTransactionConfiguration : IEntityTypeConfiguration<CreditTransaction>
    {
        public void Configure(EntityTypeBuilder<CreditTransaction> builder)
        {
            builder.HasKey(ct => ct.Id);

            builder.Property(ct => ct.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(ct => ct.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(ct => ct.ReferenceId)
                .HasMaxLength(100);

            builder.Property(ct => ct.Description)
                .HasMaxLength(500);

            builder.HasOne(ct => ct.User)
                .WithMany(u => u.CreditTransactions)
                .HasForeignKey(ct => ct.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
