using System;
using System.Threading.Tasks;

namespace TutorPlatform.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
