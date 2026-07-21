using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Auth.Commands.SetupTutorProfile
{
    public class SetupTutorProfileCommandHandler : IRequestHandler<SetupTutorProfileCommand, bool>
    {
        private readonly IAppDbContext _context;
        private readonly IFileService _fileService;

        public SetupTutorProfileCommandHandler(IAppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<bool> Handle(SetupTutorProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.TutorProfile)
                .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null || user.Role != UserRole.Tutor || user.TutorProfile == null)
            {
                throw new Exception("Invalid User or User is not a Tutor.");
            }

            // Validate Subjects exist
            var subjectIds = request.Subjects.Select(s => s.SubjectId).ToList();
            var existingSubjectsCount = await _context.Subjects
                .Where(s => subjectIds.Contains(s.Id) && s.IsActive)
                .CountAsync(cancellationToken);

            if (existingSubjectsCount != subjectIds.Count)
            {
                throw new Exception("One or more selected subjects do not exist or are inactive.");
            }

            // Upload Avatar
            if (request.AvatarFile != null)
            {
                var avatarUrl = await _fileService.UploadFileAsync(
                    request.AvatarFile, 
                    "avatars", 
                    new[] { ".jpg", ".jpeg", ".png" }, 
                    5);
                user.AvatarUrl = avatarUrl;
            }

            // Upload Qualifications
            var qualificationUrls = new List<string>();
            foreach (var qFile in request.QualificationFiles)
            {
                var url = await _fileService.UploadFileAsync(
                    qFile, 
                    "qualifications", 
                    new[] { ".jpg", ".jpeg", ".png", ".pdf" }, 
                    5);
                qualificationUrls.Add(url);
            }

            // Combine qualification text and files into JSON for storage
            var qualificationsObject = new
            {
                Text = request.QualificationsText,
                Files = qualificationUrls
            };

            // Update user
            user.FullName = request.FullName;

            // Update tutor profile
            user.TutorProfile.Bio = request.Bio;
            user.TutorProfile.SubjectsJson = JsonSerializer.Serialize(request.Subjects);
            user.TutorProfile.Qualifications = JsonSerializer.Serialize(qualificationsObject);
            user.TutorProfile.FreeSchedulesJson = request.FreeSchedulesJson;

            // Notice we do NOT change Status from Pending to Approved here.
            // Status remains what it was (Pending) until Admin approves.

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
