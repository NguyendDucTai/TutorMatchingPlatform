using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TutorPlatform.Infrastructure.Models;

namespace TutorPlatform.Infrastructure.Configurations
{
    public class SubjectConfiguration : IEntityTypeConfiguration<SubjectDataModel>
    {
        public void Configure(EntityTypeBuilder<SubjectDataModel> builder)
        {
            builder.ToTable("Subjects");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Description)
                   .HasMaxLength(500);

            builder.Property(x => x.Category)
                   .HasMaxLength(100);

            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.Category);
        }
    }
}
