using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorMatchingPlatform.Infrastructure.Models;

namespace TutorMatchingPlatform.Infrastructure.Configurations
{
    public class DepositRequestConfiguration : IEntityTypeConfiguration<DepositRequestDataModel>
    {
        public void Configure(EntityTypeBuilder<DepositRequestDataModel> builder)
        {
            builder.ToTable("DepositRequests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
