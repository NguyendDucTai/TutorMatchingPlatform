using System;
using System.Threading.Tasks;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Entities;

namespace TutorPlatform.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task<System.Collections.Generic.List<User>> GetUsersByRoleAsync(TutorPlatform.Domain.Enums.UserRole role);
        
        // For Tutor profiles mapping
        Task<TutorProfile?> GetTutorProfileAsync(Guid userId);
        Task<TutorProfile> AddTutorProfileAsync(TutorProfile profile);
        Task UpdateTutorProfileAsync(TutorProfile profile);
        Task UpdateTutorSubjectsAsync(Guid userId, IEnumerable<TutorSubject> subjects);
        
        // For Student profiles mapping
        Task<StudentProfile?> GetStudentProfileAsync(Guid userId);
        Task<StudentProfile> AddStudentProfileAsync(StudentProfile profile);
        Task UpdateStudentProfileAsync(StudentProfile profile);
    }
}
