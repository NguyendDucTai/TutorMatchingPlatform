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
                .HasColumnType("decimal(18,2)");

            // Relationship configured from User side as well
        }
    }
}
