using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TutorMatchingPlatform.Infrastructure.Data
{
    public class TutorMatchingPlatformDbContextFactory : IDesignTimeDbContextFactory<TutorMatchingPlatformDbContext>
    {
        public TutorMatchingPlatformDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TutorMatchingPlatformDbContext>();
            optionsBuilder.UseSqlServer("Data Source=PCCUATAI;Initial Catalog=TutorMatchingPlatformDb;Integrated Security=True;Trust Server Certificate=True");

            return new TutorMatchingPlatformDbContext(optionsBuilder.Options);
        }
    }
}
