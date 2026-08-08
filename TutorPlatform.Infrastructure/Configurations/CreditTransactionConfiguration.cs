using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class CreditTransactionConfiguration : IEntityTypeConfiguration<CreditTransactionDataModel>
    {
        public void Configure(EntityTypeBuilder<CreditTransactionDataModel> builder)
        {
            builder.ToTable("CreditTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Description)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.Amount)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.BalanceAfter)
                   .HasColumnType("decimal(18,2)");

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.BookingId);
            builder.HasIndex(x => x.CreatedAt);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.CreditTransactions)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Booking)
                   .WithMany(x => x.CreditTransactions)
                   .HasForeignKey(x => x.BookingId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
