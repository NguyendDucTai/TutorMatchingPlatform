using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using TutorMatchingPlatform.Application.Sessions.Commands.ProposeSessionChange;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;
using Xunit;

namespace TutorMatchingPlatform.Application.UnitTests
{
    public class SessionChangeTests
    {
        [Fact]
        public async Task ProposeSessionChange_MoreThan24HoursInAdvance_NoLateFee_BR03()
        {
            // Arrange
            using var context = TestDbContext.Create();

            var studentUser = new User { Id = 1, FullName = "Student", Role = UserRole.Student, CreditBalance = 1000m, StudentProfile = new StudentProfile { Id = 10 } };
            var tutorUser = new User { Id = 2, FullName = "Tutor", Role = UserRole.Tutor, TutorProfile = new TutorProfile { Id = 20 } };
            studentUser.StudentProfile.User = studentUser;
            tutorUser.TutorProfile.User = tutorUser;

            // Session start time is 48 hours in the future (>= 24h)
            var session = new Session
            {
                Id = 100,
                StudentId = 10,
                TutorId = 20,
                Student = studentUser.StudentProfile,
                Tutor = tutorUser.TutorProfile,
                StartTime = DateTime.UtcNow.AddHours(48),
                EndTime = DateTime.UtcNow.AddHours(50),
                Status = SessionStatus.Confirmed
            };

            var originalTransaction = new CreditTransaction
            {
                Id = 1,
                UserId = 1,
                Amount = -200m,
                Type = CreditTransactionType.SessionFee,
                ReferenceId = "100"
            };

            context.Users.AddRange(studentUser, tutorUser);
            context.Sessions.Add(session);
            context.CreditTransactions.Add(originalTransaction);
            await context.SaveChangesAsync();

            var handler = new ProposeSessionChangeCommandHandler(context);

            var command = new ProposeSessionChangeCommand
            {
                SessionId = 100,
                RequesterUserId = 1,
                ChangeType = SessionChangeType.Cancel
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.IsLateCancellation.Should().BeFalse();
            studentUser.CreditBalance.Should().Be(1000m); // Balance unchanged (no late fee deducted)
        }

        [Fact]
        public async Task ProposeSessionChange_LessThan24HoursInAdvance_Deducts30PercentLateFee_BR10()
        {
            // Arrange
            using var context = TestDbContext.Create();

            var studentUser = new User { Id = 1, FullName = "Student", Role = UserRole.Student, CreditBalance = 1000m, StudentProfile = new StudentProfile { Id = 10 } };
            var tutorUser = new User { Id = 2, FullName = "Tutor", Role = UserRole.Tutor, TutorProfile = new TutorProfile { Id = 20 } };
            studentUser.StudentProfile.User = studentUser;
            tutorUser.TutorProfile.User = tutorUser;

            // Session start time is 5 hours in the future (< 24h) -> Late cancellation!
            var session = new Session
            {
                Id = 100,
                StudentId = 10,
                TutorId = 20,
                Student = studentUser.StudentProfile,
                Tutor = tutorUser.TutorProfile,
                StartTime = DateTime.UtcNow.AddHours(5),
                EndTime = DateTime.UtcNow.AddHours(7),
                Status = SessionStatus.Confirmed
            };

            var originalTransaction = new CreditTransaction
            {
                Id = 1,
                UserId = 1,
                Amount = -200m, // Session fee = 200 credits
                Type = CreditTransactionType.SessionFee,
                ReferenceId = "100"
            };

            context.Users.AddRange(studentUser, tutorUser);
            context.Sessions.Add(session);
            context.CreditTransactions.Add(originalTransaction);
            await context.SaveChangesAsync();

            var handler = new ProposeSessionChangeCommandHandler(context);

            var command = new ProposeSessionChangeCommand
            {
                SessionId = 100,
                RequesterUserId = 1,
                ChangeType = SessionChangeType.Cancel
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.IsLateCancellation.Should().BeTrue();

            // 30% of 200 credits = 60 credits late fee deducted
            studentUser.CreditBalance.Should().Be(940m); // 1000 - 60 = 940
        }
    }
}
