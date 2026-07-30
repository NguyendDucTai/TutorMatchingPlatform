using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using TutorMatchingPlatform.Application.Feedbacks.Commands.RateSession;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;
using Xunit;

namespace TutorMatchingPlatform.Application.UnitTests
{
    public class RateSessionTests
    {
        [Fact]
        public async Task RateSession_WhenSessionNotCompleted_ReturnsError_BR05()
        {
            // Arrange
            using var context = TestDbContext.Create();

            var studentUser = new User { Id = 1, Role = UserRole.Student, StudentProfile = new StudentProfile { Id = 10 } };
            var tutorUser = new User { Id = 2, Role = UserRole.Tutor, TutorProfile = new TutorProfile { Id = 20 } };

            var session = new Session
            {
                Id = 100,
                StudentId = 10,
                TutorId = 20,
                Student = studentUser.StudentProfile,
                Tutor = tutorUser.TutorProfile,
                Status = SessionStatus.Confirmed // Not Completed!
            };

            context.Users.AddRange(studentUser, tutorUser);
            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            var handler = new RateSessionCommandHandler(context);

            var command = new RateSessionCommand
            {
                SessionId = 100,
                SenderUserId = 1,
                Rating = 5,
                Comment = "Great session!"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("completed");
        }

        [Fact]
        public async Task RateSession_WhenAlreadyRated_ReturnsError_BR06()
        {
            // Arrange
            using var context = TestDbContext.Create();

            var studentUser = new User { Id = 1, Role = UserRole.Student, StudentProfile = new StudentProfile { Id = 10 } };
            var tutorUser = new User { Id = 2, Role = UserRole.Tutor, TutorProfile = new TutorProfile { Id = 20 } };
            studentUser.StudentProfile.User = studentUser;
            tutorUser.TutorProfile.User = tutorUser;

            var session = new Session
            {
                Id = 100,
                StudentId = 10,
                TutorId = 20,
                Student = studentUser.StudentProfile,
                Tutor = tutorUser.TutorProfile,
                Status = SessionStatus.Completed
            };

            var existingFeedback = new Feedback
            {
                Id = 1,
                SessionId = 100,
                SenderId = 1,
                ReceiverId = 2,
                Rating = 4,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(studentUser, tutorUser);
            context.Sessions.Add(session);
            context.Feedbacks.Add(existingFeedback);
            await context.SaveChangesAsync();

            var handler = new RateSessionCommandHandler(context);

            var command = new RateSessionCommand
            {
                SessionId = 100,
                SenderUserId = 1,
                Rating = 5
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already rated");
        }

        [Fact]
        public async Task RateSession_Success_RecalculatesReputationScoreOverLast90Days_BR07()
        {
            // Arrange
            using var context = TestDbContext.Create();

            var studentUser = new User { Id = 1, Role = UserRole.Student, StudentProfile = new StudentProfile { Id = 10 } };
            var tutorUser = new User { Id = 2, Role = UserRole.Tutor, TutorProfile = new TutorProfile { Id = 20, ReputationScore = 0.0 } };
            studentUser.StudentProfile.User = studentUser;
            tutorUser.TutorProfile.User = tutorUser;

            var session = new Session
            {
                Id = 100,
                StudentId = 10,
                TutorId = 20,
                Student = studentUser.StudentProfile,
                Tutor = tutorUser.TutorProfile,
                Status = SessionStatus.Completed
            };

            // Existing rating 3 days ago (Rating = 4)
            var previousFeedback = new Feedback
            {
                Id = 1,
                SessionId = 99,
                SenderId = 3,
                ReceiverId = 2,
                Rating = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            };

            // Old rating 100 days ago (Rating = 1) -> Should be IGNORED by BR-07 90-day window!
            var oldFeedback = new Feedback
            {
                Id = 2,
                SessionId = 98,
                SenderId = 4,
                ReceiverId = 2,
                Rating = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-100)
            };

            context.Users.AddRange(studentUser, tutorUser);
            context.Sessions.Add(session);
            context.Feedbacks.AddRange(previousFeedback, oldFeedback);
            await context.SaveChangesAsync();

            var handler = new RateSessionCommandHandler(context);

            // New rating = 5
            var command = new RateSessionCommand
            {
                SessionId = 100,
                SenderUserId = 1,
                Rating = 5,
                Comment = "Awesome tutor!"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            // Reputation score should be average of recent 2 ratings: (4 + 5) / 2 = 4.5
            // Old feedback (100 days ago) must be excluded per BR-07
            tutorUser.TutorProfile.ReputationScore.Should().Be(4.5);
        }
    }
}
