using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Enums;
using TutorPlatform.Domain.Interfaces;
using TutorPlatform.Infrastructure.Models;
using TutorPlatform.Infrastructure.Persistence;

namespace TutorPlatform.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            var dataModel = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (dataModel == null) return null;

            return MapToDomain(dataModel);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var dataModel = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (dataModel == null) return null;

            return MapToDomain(dataModel);
        }

        public async Task<User> AddAsync(User user)
        {
            var dataModel = new UserDataModel
            {
                Id = user.Id,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                FullName = user.FullName,
                Role = (int)user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
                AdminNote = user.AdminNote
            };

            await _dbContext.Users.AddAsync(dataModel);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        public async Task UpdateAsync(User user)
        {
            var dataModel = await _dbContext.Users.FindAsync(user.Id);
            if (dataModel != null)
            {
                dataModel.Email = user.Email;
                dataModel.PasswordHash = user.PasswordHash;
                dataModel.FullName = user.FullName;
                dataModel.Phone = user.Phone;
                dataModel.AvatarUrl = user.AvatarUrl;
                dataModel.Role = (int)user.Role;
                dataModel.IsActive = user.IsActive;
                dataModel.CreditBalance = user.CreditBalance;
                dataModel.UpdatedAt = user.UpdatedAt;
                dataModel.RefreshToken = user.RefreshToken;
                dataModel.RefreshTokenExpiryTime = user.RefreshTokenExpiryTime;
                dataModel.AdminNote = user.AdminNote;

                _dbContext.Users.Update(dataModel);
                await _dbContext.SaveChangesAsync();
            }
        }        public async Task<System.Collections.Generic.List<User>> GetUsersByRoleAsync(TutorPlatform.Domain.Enums.UserRole role)
        {
            var dataModels = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Role == (int)role)
                .ToListAsync();

            return dataModels.Select(MapToDomain).ToList();
        }

        public async Task<TutorProfile?> GetTutorProfileAsync(Guid userId)
        {
            var dataModel = await _dbContext.TutorProfiles
                .Include(tp => tp.TutorSubjects)
                .ThenInclude(ts => ts.Subject)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (dataModel == null) return null;

            var profile = new TutorProfile(dataModel.UserId, dataModel.Bio, dataModel.Qualifications);
            SetProperty(profile, "Id", dataModel.Id);
            if (dataModel.ApprovalStatus == 1) profile.Approve(dataModel.ApprovedBy ?? Guid.Empty);
            else if (dataModel.ApprovalStatus == 2) profile.Reject();
            SetProperty(profile, "ApprovalStatus", dataModel.ApprovalStatus);
            
            if (dataModel.DefaultMeetingLink != null)
            {
                profile.UpdateDefaultMeetingLink(dataModel.DefaultMeetingLink);
            }
            
            SetProperty(profile, "AverageRating", dataModel.AverageRating);
            SetProperty(profile, "TotalReviews", dataModel.TotalReviews);
            SetProperty(profile, "TotalSessions", dataModel.TotalSessions);

            // Map TutorSubjects from DB to domain entity
            foreach (var tsData in dataModel.TutorSubjects)
            {
                var tutorSubject = new TutorSubject(
                    tsData.TutorProfileId,
                    tsData.SubjectId,
                    (ProficiencyLevel)tsData.ProficiencyLevel,
                    tsData.HourlyCredits);
                SetProperty(tutorSubject, "Id", tsData.Id);
                profile.AddTutorSubject(tutorSubject);
            }
            
            return profile;
        }

        public async Task<TutorProfile> AddTutorProfileAsync(TutorProfile profile)
        {
            var existing = await _dbContext.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == profile.UserId);
            if (existing != null)
            {
                return profile;
            }

            var dataModel = new TutorProfileDataModel
            {
                Id = profile.Id != Guid.Empty ? profile.Id : Guid.NewGuid(),
                UserId = profile.UserId,
                Bio = profile.Bio,
                Qualifications = profile.Qualifications,
                IsApproved = false,
                ApprovalStatus = 0,
                ApprovedAt = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.TutorProfiles.AddAsync(dataModel);
            await _dbContext.SaveChangesAsync();

            return profile;
        }

        public async Task UpdateTutorProfileAsync(TutorProfile profile)
        {
            var dataModel = await _dbContext.TutorProfiles
                .FirstOrDefaultAsync(p => p.UserId == profile.UserId || p.Id == profile.Id);

            if (dataModel == null)
            {
                dataModel = new TutorProfileDataModel
                {
                    Id = profile.Id != Guid.Empty ? profile.Id : Guid.NewGuid(),
                    UserId = profile.UserId,
                    Bio = profile.Bio,
                    Qualifications = profile.Qualifications,
                    DefaultMeetingLink = profile.DefaultMeetingLink,
                    IsApproved = false,
                    ApprovalStatus = 0,
                    ApprovedAt = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _dbContext.TutorProfiles.AddAsync(dataModel);
            }
            else
            {
                dataModel.Bio = profile.Bio;
                dataModel.Qualifications = profile.Qualifications;
                dataModel.DefaultMeetingLink = profile.DefaultMeetingLink;
                if (dataModel.ApprovalStatus == 2) // Resubmit on update
                {
                    dataModel.ApprovalStatus = 0;
                    dataModel.IsApproved = false;
                }
                else
                {
                    dataModel.ApprovalStatus = profile.ApprovalStatus;
                    dataModel.IsApproved = profile.IsApproved;
                }
                dataModel.ApprovedAt = profile.ApprovedAt;
                dataModel.AverageRating = profile.AverageRating;
                dataModel.TotalReviews = profile.TotalReviews;
                dataModel.TotalSessions = profile.TotalSessions;
                dataModel.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateTutorSubjectsAsync(Guid userId, IEnumerable<TutorSubject> subjects)
        {
            var dataModel = await _dbContext.TutorProfiles
                .Include(tp => tp.TutorSubjects)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (dataModel == null) return;

            var domainSubjectIds = subjects.Select(ts => ts.SubjectId).ToList();

            // 1. Remove subjects that are no longer in the domain entity
            var subjectsToRemove = dataModel.TutorSubjects.Where(ts => !domainSubjectIds.Contains(ts.SubjectId)).ToList();
            foreach (var subjectToRemove in subjectsToRemove)
            {
                dataModel.TutorSubjects.Remove(subjectToRemove);
                _dbContext.TutorSubjects.Remove(subjectToRemove);
            }

            // 2. Add or update subjects from the domain entity
            foreach (var domainSubject in subjects)
            {
                var existingSubject = dataModel.TutorSubjects.FirstOrDefault(ts => ts.SubjectId == domainSubject.SubjectId);
                if (existingSubject == null)
                {
                    var newSubject = new TutorSubjectDataModel
                    {
                        Id = Guid.NewGuid(),
                        TutorProfileId = dataModel.Id,
                        SubjectId = domainSubject.SubjectId,
                        ProficiencyLevel = (int)domainSubject.ProficiencyLevel,
                        HourlyCredits = domainSubject.HourlyCredits,
                        CreatedAt = DateTime.UtcNow
                    };
                    dataModel.TutorSubjects.Add(newSubject);
                    _dbContext.Entry(newSubject).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                else
                {
                    existingSubject.ProficiencyLevel = (int)domainSubject.ProficiencyLevel;
                    existingSubject.HourlyCredits = domainSubject.HourlyCredits;
                }
            }

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Exception message: {ex.Message}");
                foreach (var entry in ex.Entries)
                {
                    var entityType = entry.Entity.GetType().Name;
                    var state = entry.State.ToString();
                    var keys = string.Join(", ", entry.Metadata.FindPrimaryKey().Properties.Select(p => $"{p.Name} = {entry.Property(p.Name).CurrentValue}"));
                    sb.AppendLine($"Entity: {entityType}, State: {state}, Keys: {keys}");
                    
                    sb.AppendLine("Properties:");
                    foreach (var prop in entry.Metadata.GetProperties())
                    {
                        var original = entry.Property(prop.Name).OriginalValue;
                        var current = entry.Property(prop.Name).CurrentValue;
                        sb.AppendLine($"  {prop.Name}: Original = '{original}', Current = '{current}'");
                    }
                }
                System.IO.File.WriteAllText("d:/TutorMatching/TutorMatching.Backend/concurrency_error.txt", sb.ToString());
                throw;
            }
        }

        public async Task<StudentProfile?> GetStudentProfileAsync(Guid userId)
        {
            var dataModel = await _dbContext.StudentProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (dataModel == null) return null;

            var profile = new StudentProfile(dataModel.UserId, dataModel.GradeLevel, dataModel.LearningPreferences);
            SetProperty(profile, "Id", dataModel.Id);
            return profile;
        }

        public async Task<StudentProfile> AddStudentProfileAsync(StudentProfile profile)
        {
            var dataModel = new StudentProfileDataModel
            {
                Id = profile.Id,
                UserId = profile.UserId,
                GradeLevel = profile.GradeLevel,
                LearningPreferences = profile.LearningPreferences,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };

            await _dbContext.StudentProfiles.AddAsync(dataModel);
            await _dbContext.SaveChangesAsync();

            return profile;
        }

        public async Task UpdateStudentProfileAsync(StudentProfile profile)
        {
            var dataModel = await _dbContext.StudentProfiles.FindAsync(profile.Id);
            if (dataModel != null)
            {
                dataModel.GradeLevel = profile.GradeLevel;
                dataModel.LearningPreferences = profile.LearningPreferences;
                dataModel.UpdatedAt = profile.UpdatedAt;

                _dbContext.StudentProfiles.Update(dataModel);
                await _dbContext.SaveChangesAsync();
            }
        }

        private User MapToDomain(UserDataModel dataModel)
        {
            var user = new User(dataModel.Email, dataModel.PasswordHash, dataModel.FullName, dataModel.Phone, dataModel.AvatarUrl, (UserRole)dataModel.Role);
            
            SetProperty(user, "Id", dataModel.Id);
            SetProperty(user, "CreditBalance", dataModel.CreditBalance);
            
            if (dataModel.RefreshToken != null && dataModel.RefreshTokenExpiryTime.HasValue)
            {
                user.SetRefreshToken(dataModel.RefreshToken, dataModel.RefreshTokenExpiryTime.Value);
            }
            
            if (!dataModel.IsActive)
            {
                user.Deactivate();
            }

            if (dataModel.AdminNote != null)
            {
                user.UpdateAdminNote(dataModel.AdminNote);
            }

            return user;
        }

        private static void SetProperty<T>(T obj, string propertyName, object? value)
        {
            var prop = typeof(T).GetProperty(propertyName);
            if (prop != null)
            {
                var setMethod = prop.GetSetMethod(true);
                if (setMethod != null)
                {
                    setMethod.Invoke(obj, new[] { value });
                }
                else
                {
                    prop.SetValue(obj, value);
                }
            }
        }
    }
}
