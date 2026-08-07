using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Domain.Interfaces
{
    public interface ICreditTransactionRepository
    {
        Task<CreditTransaction?> GetByIdAsync(Guid id);
        Task<PagedResult<CreditTransaction>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<CreditTransaction> AddAsync(CreditTransaction transaction);
    }
}
