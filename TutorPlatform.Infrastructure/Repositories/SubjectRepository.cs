using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorPlatform.Domain.Entities;
using TutorPlatform.Domain.Interfaces;
using TutorPlatform.Infrastructure.Models;
using TutorPlatform.Infrastructure.Persistence;
using TutorPlatform.Domain.Common;

namespace TutorPlatform.Infrastructure.Repositories
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly ApplicationDbContext _context;

        public SubjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Subject?> GetByIdAsync(Guid id)
        {
            var dataModel = await _context.Subjects.FindAsync(id);
            return dataModel != null ? MapToDomain(dataModel) : null;
        }

        public async Task<Subject?> GetByNameAsync(string name)
        {
            var dataModel = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == name);
            return dataModel != null ? MapToDomain(dataModel) : null;
        }

        public async Task<IReadOnlyList<Subject>> GetAllActiveAsync()
        {
            var dataModels = await _context.Subjects.Where(s => s.IsActive).ToListAsync();
            return dataModels.Select(MapToDomain).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Subject>> GetAllAsync()
        {
            var dataModels = await _context.Subjects.ToListAsync();
            return dataModels.Select(MapToDomain).ToList().AsReadOnly();
        }

        public async Task<PagedResult<Subject>> GetPagedAsync(int pageNumber, int pageSize, string? search)
        {
            var query = _context.Subjects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.Name.Contains(search) || (s.Description != null && s.Description.Contains(search)));
            }

            var totalCount = await query.CountAsync();
            var dataModels = await query
                .OrderBy(s => s.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var subjects = dataModels.Select(MapToDomain).ToList();
            return new PagedResult<Subject>(subjects.AsReadOnly(), totalCount);
        }

        public async Task<Subject> AddAsync(Subject subject)
        {
            var dataModel = new SubjectDataModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Description = subject.Description,
                IsActive = subject.IsActive,
                CreatedAt = subject.CreatedAt,
                UpdatedAt = subject.UpdatedAt
            };

            _context.Subjects.Add(dataModel);
            await _context.SaveChangesAsync();
            return subject;
        }

        public async Task UpdateAsync(Subject subject)
        {
            var dataModel = await _context.Subjects.FindAsync(subject.Id);
            if (dataModel != null)
            {
                dataModel.Name = subject.Name;
                dataModel.Description = subject.Description;
                dataModel.IsActive = subject.IsActive;
                dataModel.UpdatedAt = subject.UpdatedAt;
                
                _context.Subjects.Update(dataModel);
                await _context.SaveChangesAsync();
            }
        }

        private Subject MapToDomain(SubjectDataModel dataModel)
        {
            var subject = new Subject(dataModel.Name, dataModel.Description, dataModel.Category);
            var type = typeof(Subject);
            type.GetProperty("Id")?.SetValue(subject, dataModel.Id);
            if (!dataModel.IsActive) subject.Deactivate(); // Need to assume Subject has Deactivate
            return subject;
        }
    }
}
