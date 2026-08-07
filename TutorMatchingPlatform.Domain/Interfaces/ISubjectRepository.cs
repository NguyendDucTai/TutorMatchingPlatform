using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TutorMatchingPlatform.Domain.Common;
using TutorMatchingPlatform.Domain.Entities;

namespace TutorMatchingPlatform.Domain.Interfaces
{
    public interface ISubjectRepository
    {
        Task<Subject?> GetByIdAsync(Guid id);
        Task<Subject?> GetByNameAsync(string name);
        Task<IReadOnlyList<Subject>> GetAllActiveAsync();
        Task<IReadOnlyList<Subject>> GetAllAsync();
        Task<PagedResult<Subject>> GetPagedAsync(int pageNumber, int pageSize, string? search);
        Task<Subject> AddAsync(Subject subject);
        Task UpdateAsync(Subject subject);
    }
}
