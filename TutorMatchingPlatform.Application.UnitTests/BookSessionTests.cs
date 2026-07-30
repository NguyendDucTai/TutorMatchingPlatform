using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Application.Sessions.Commands.BookSession;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;
using Xunit;

namespace TutorMatchingPlatform.Application.UnitTests
{
    public class BookSessionTests
    {
        private readonly Mock<IEmailService> _emailServiceMock;

        public BookSessionTests()
        {
            _emailServiceMock = new Mock<IEmailService>();
        }

        [Fact]
        public async Task BookSession_WhenTimeSlotOverlaps_ReturnsScheduleConflict_BR02()
        {
            // Arrange
            using var context = TestDbContext.Create();

            var tutorUser = new User
            {
                Id = 1,
                FullName = "Tutor One",
                Email = "tutor@test.com",
                Role = UserRole.Tutor,
                TutorProfile = new TutorProfile
                {
                    Id = 10,
                    Status = ProfileStatus.Approved,
                    SubjectsJson = JsonSerializer.Serialize(new List<object>
                    {
                        new { SubjectId = 1, Rate = 100m }
                    })
                }
            };

            var studentUser = new User
            {
                Id = 2,
                FullName = "Student One",
                Email = "student@test.com",
                Role = UserRole.Student,
                CreditBalance = 500m,
                StudentProfile = new StudentProfile { Id = 20 }
            };

            context.Users.AddRange(tutorUser, studentUser);

            // Existing booking from 10:00 to 12:00
            var existingSession = new Session
            {
                Id = 100,
                TutorId = 10,
                StudentId = 20,
                SubjectId = 1,
                StartTime = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                Status = SessionStatus.Confirmed
            };
            context.Sessions.Add(existingSession);
            await context.SaveChangesAsync();

            var handler = new BookSessionCommandHandler(context, _emailServiceMock.Object);

            // Attempt to book overlapping time slot: 11:00 to 13:00 (overlaps with 10:00-12:00)
            var command = new BookSessionCommand
            {
                StudentId = 2,
                TutorId = 1,
                SubjectId = 1,
                StartTime = new DateTime(2026, 8, 1, 11, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 8, 1, 13, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already booked");
        }

        [Fact]
        public async Task BookSession_WhenStudentHasInsufficientCredits_ReturnsInsufficientCredits_BR12()
        {
            // Arrange
            using var context = TestDbContext.Create();

            var tutorUser = new User
            {
                Id = 1,
                FullName = "Tutor One",
                Role = UserRole.Tutor,
                TutorProfile = new TutorProfile
                {
                    Id = 10,
                    Status = ProfileStatus.Approved,
                    SubjectsJson = JsonSerializer.Serialize(new List<object>
                    {
                        new { SubjectId = 1, Rate = 200m }
                    })
                }
            };

            var studentUser = new User
            {
                Id = 2,
                FullName = "Student Poor",
                Role = UserRole.Student,
                CreditBalance = 50m, // Only 50 credits, session costs 200 credits
                StudentProfile = new StudentProfile { Id = 20 }
            };

            context.Users.AddRange(tutorUser, studentUser);
            await context.SaveChangesAsync();

            var handler = new BookSessionCommandHandler(context, _emailServiceMock.Object);

            var command = new BookSessionCommand
            {
                StudentId = 2,
                TutorId = 1,
                SubjectId = 1,
                StartTime = new DateTime(2026, 8, 2, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 8, 2, 11, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Insufficient credits");
        }

        [Fact]
        public async Task BookSession_WhenValid_DeductsCreditsAndConfirmsSession()
        {
            // Arrange
            using var context = TestDbContext.Create();

            var tutorUser = new User
            {
                Id = 1,
                FullName = "Tutor One",
                Email = "tutor@test.com",
                Role = UserRole.Tutor,
                TutorProfile = new TutorProfile
                {
                    Id = 10,
                    Status = ProfileStatus.Approved,
                    SubjectsJson = JsonSerializer.Serialize(new List<object>
                    {
                        new { SubjectId = 1, Rate = 150m }
                    })
                }
            };

            var studentUser = new User
            {
                Id = 2,
                FullName = "Student Rich",
                Email = "student@test.com",
                Role = UserRole.Student,
                CreditBalance = 500m,
                StudentProfile = new StudentProfile { Id = 20 }
            };

            context.Users.AddRange(tutorUser, studentUser);
            await context.SaveChangesAsync();

            var handler = new BookSessionCommandHandler(context, _emailServiceMock.Object);

            var command = new BookSessionCommand
            {
                StudentId = 2,
                TutorId = 1,
                SubjectId = 1,
                StartTime = new DateTime(2026, 8, 5, 14, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 8, 5, 15, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            studentUser.CreditBalance.Should().Be(350m); // 500 - 150
            result.MeetingLink.Should().NotBeNullOrEmpty();
        }
    }
}
