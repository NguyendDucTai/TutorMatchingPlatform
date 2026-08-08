using System.Threading.Tasks;
using TutorPlatform.Application.Common.Interfaces;
using TutorPlatform.Infrastructure.Persistence;

namespace TutorPlatform.Infrastructure.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task BeginTransactionAsync()
        {
            if (_dbContext.Database.CurrentTransaction == null)
            {
                await _dbContext.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitTransactionAsync()
        {
            if (_dbContext.Database.CurrentTransaction != null)
            {
                await _dbContext.Database.CurrentTransaction.CommitAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_dbContext.Database.CurrentTransaction != null)
            {
                await _dbContext.Database.CurrentTransaction.RollbackAsync();
            }
        }
    }
}
