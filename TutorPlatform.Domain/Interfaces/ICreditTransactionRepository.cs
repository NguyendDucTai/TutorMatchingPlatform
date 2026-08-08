using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Entities;

namespace TutorPlatform.Domain.Interfaces
{
    public interface ICreditTransactionRepository
    {
        Task<CreditTransaction?> GetByIdAsync(Guid id);
        Task<PagedResult<CreditTransaction>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<CreditTransaction> AddAsync(CreditTransaction transaction);
    }
}
