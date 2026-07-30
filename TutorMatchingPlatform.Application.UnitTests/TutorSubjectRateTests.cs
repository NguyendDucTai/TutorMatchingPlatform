using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using TutorMatchingPlatform.Application.Profiles.Commands.UpdateTutorSubjects;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;
using Xunit;

namespace TutorMatchingPlatform.Application.UnitTests
{
    public class TutorSubjectRateTests
    {
        [Fact]
        public async Task UpdateTutorSubjects_ValidRate_UpdatesSubjectsAndResetsStatusToPending_BR11()
        {
            // Arrange
            using var context = TestDbContext.Create();

            var tutorUser = new User
            {
                Id = 1,
                FullName = "Tutor Teacher",
                Role = UserRole.Tutor,
                TutorProfile = new TutorProfile
                {
                    Id = 10,
                    Status = ProfileStatus.Approved,
                    SubjectsJson = null
                }
            };

            context.Users.Add(tutorUser);
            await context.SaveChangesAsync();

            var handler = new UpdateTutorSubjectsCommandHandler(context);

            var command = new UpdateTutorSubjectsCommand
            {
                UserId = 1,
                Subjects = new List<SubjectRateDto>
                {
                    new SubjectRateDto { SubjectId = 1, Rate = 150m }, // BR-11: Rate = 150 (> 0)
                    new SubjectRateDto { SubjectId = 2, Rate = 200m }
                }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            tutorUser.TutorProfile.SubjectsJson.Should().Contain("150");
            tutorUser.TutorProfile.SubjectsJson.Should().Contain("200");
            
            // Re-evaluation required on profile change: status moves back to Pending for Admin review
            tutorUser.TutorProfile.Status.Should().Be(ProfileStatus.Pending);
        }

        [Fact]
        public async Task UpdateTutorSubjects_NonExistentUser_ReturnsFalse()
        {
            // Arrange
            using var context = TestDbContext.Create();
            var handler = new UpdateTutorSubjectsCommandHandler(context);

            var command = new UpdateTutorSubjectsCommand
            {
                UserId = 999, // User does not exist
                Subjects = new List<SubjectRateDto>()
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }
    }
}
