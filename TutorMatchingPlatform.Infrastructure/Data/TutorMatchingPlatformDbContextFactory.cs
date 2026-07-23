using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TutorMatchingPlatform.Infrastructure.Data
{
    public class TutorMatchingPlatformDbContextFactory : IDesignTimeDbContextFactory<TutorMatchingPlatformDbContext>
    {
        public TutorMatchingPlatformDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TutorMatchingPlatformDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TutorMatchingDB;Trusted_Connection=True;MultipleActiveResultSets=true");

            return new TutorMatchingPlatformDbContext(optionsBuilder.Options);
        }
    }
}
