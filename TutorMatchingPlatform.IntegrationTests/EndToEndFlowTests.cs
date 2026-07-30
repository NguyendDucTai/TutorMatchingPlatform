using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TutorMatchingPlatform.Application.Interfaces.Authentication;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;
using TutorMatchingPlatform.Infrastructure.Data;
using Xunit;

namespace TutorMatchingPlatform.IntegrationTests
{
    public class EndToEndFlowTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public EndToEndFlowTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Full_EndToEnd_Lifecycle_Flow_Success()
        {
            // -------------------------------------------------------------
            // Step 0: Seed Admin & Subject 1
            // -------------------------------------------------------------
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TutorMatchingPlatformDbContext>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

                db.Database.EnsureCreated();

                if (!db.Users.Any(u => u.Role == UserRole.Administrator))
                {
                    db.Users.Add(new User
                    {
                        FullName = "System Administrator",
                        Email = "admin@tutormatching.com",
                        PasswordHash = passwordHasher.HashPassword("Admin@123"),
                        Role = UserRole.Administrator,
                        IsSuspended = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (!db.Subjects.Any(s => s.Id == 1))
                {
                    db.Subjects.Add(new Subject
                    {
                        Id = 1,
                        Name = "Mathematics",
                        Description = "High school math",
                        IsActive = true
                    });
                }

                await db.SaveChangesAsync();
            }

            // -------------------------------------------------------------
            // Step 1: Login Admin
            // -------------------------------------------------------------
            var adminLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "admin@tutormatching.com",
                password = "Admin@123"
            });
            if (adminLoginResponse.StatusCode != HttpStatusCode.OK)
            {
                var errorText = await adminLoginResponse.Content.ReadAsStringAsync();
                throw new Exception($"Admin Login failed ({adminLoginResponse.StatusCode}): {errorText}");
            }
            var adminAuthJson = await adminLoginResponse.Content.ReadFromJsonAsync<JsonElement>();
            string adminToken = adminAuthJson.GetProperty("token").GetString()!;

            // -------------------------------------------------------------
            // Step 2: Register Tutor
            // -------------------------------------------------------------
            var tutorRegisterResponse = await _client.PostAsJsonAsync("/api/auth/register", new
            {
                fullName = "E2E Tutor Professional",
                email = $"tutor.{Guid.NewGuid().ToString().Substring(0, 5)}@test.com",
                password = "Password@123",
                confirmPassword = "Password@123",
                role = 1 // Tutor (1)
            });
            if (tutorRegisterResponse.StatusCode != HttpStatusCode.OK)
            {
                var err = await tutorRegisterResponse.Content.ReadAsStringAsync();
                throw new Exception($"Tutor Register failed ({tutorRegisterResponse.StatusCode}): {err}");
            }
            var tutorAuthJson = await tutorRegisterResponse.Content.ReadFromJsonAsync<JsonElement>();
            string tutorToken = tutorAuthJson.GetProperty("token").GetString()!;
            int tutorUserId = tutorAuthJson.GetProperty("user").GetProperty("id").GetInt32();

            // -------------------------------------------------------------
            // Step 3: Register Student
            // -------------------------------------------------------------
            var studentRegisterResponse = await _client.PostAsJsonAsync("/api/auth/register", new
            {
                fullName = "E2E Student Learner",
                email = $"student.{Guid.NewGuid().ToString().Substring(0, 5)}@test.com",
                password = "Password@123",
                confirmPassword = "Password@123",
                role = 0 // Student (0)
            });
            if (studentRegisterResponse.StatusCode != HttpStatusCode.OK)
            {
                var err = await studentRegisterResponse.Content.ReadAsStringAsync();
                throw new Exception($"Student Register failed ({studentRegisterResponse.StatusCode}): {err}");
            }
            var studentAuthJson = await studentRegisterResponse.Content.ReadFromJsonAsync<JsonElement>();
            string studentToken = studentAuthJson.GetProperty("token").GetString()!;
            int studentUserId = studentAuthJson.GetProperty("user").GetProperty("id").GetInt32();

            // -------------------------------------------------------------
            // Step 4: Tutor Updates Subjects & Avatar (Fulfills Approval Requirements)
            // -------------------------------------------------------------
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TutorMatchingPlatformDbContext>();
                var tutorUser = await db.Users.FindAsync(tutorUserId);
                if (tutorUser != null)
                {
                    tutorUser.AvatarUrl = "/uploads/avatars/test-avatar.jpg";
                    await db.SaveChangesAsync();
                }
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tutorToken);
            var subjectsResponse = await _client.PostAsJsonAsync("/api/profiles/tutor/subjects", new[]
            {
                new { subjectId = 1, rate = 150m }
            });
            subjectsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Get Tutor Profile Id
            int tutorProfileId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TutorMatchingPlatformDbContext>();
                var tutorUser = await db.Users.Include(u => u.TutorProfile).FirstOrDefaultAsync(u => u.Id == tutorUserId);
                tutorProfileId = tutorUser!.TutorProfile!.Id;
            }

            // -------------------------------------------------------------
            // Step 5: Admin Approves Tutor Profile
            // -------------------------------------------------------------
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            var approveResponse = await _client.PostAsync($"/api/admin/tutor-profiles/{tutorProfileId}/approve", null);
            approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // -------------------------------------------------------------
            // Step 6: Public Search Tutors (AllowAnonymous)
            // -------------------------------------------------------------
            _client.DefaultRequestHeaders.Authorization = null;
            var searchResponse = await _client.GetAsync("/api/tutor/search?subjectId=1");
            searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // -------------------------------------------------------------
            // Step 7: Student Requests Deposit & Admin Approves
            // -------------------------------------------------------------
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);
            var depositResponse = await _client.PostAsJsonAsync("/api/credits/deposit", new
            {
                amount = 500m,
                note = "E2E Test Top-up"
            });
            depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            int creditRequestId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TutorMatchingPlatformDbContext>();
                var req = await db.CreditRequests.FirstOrDefaultAsync(c => c.UserId == studentUserId);
                creditRequestId = req!.Id;
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            var approveDepositResponse = await _client.PostAsync($"/api/admin/credits/{creditRequestId}/approve", null);
            approveDepositResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // -------------------------------------------------------------
            // Step 8: Student Books a Session
            // -------------------------------------------------------------
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);
            var bookingStartTime = DateTime.UtcNow.AddDays(2);
            var bookingEndTime = bookingStartTime.AddHours(1);

            var bookResponse = await _client.PostAsJsonAsync("/api/session/book", new
            {
                tutorId = tutorUserId,
                subjectId = 1,
                startTime = bookingStartTime,
                endTime = bookingEndTime
            });
            bookResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookJson = await bookResponse.Content.ReadFromJsonAsync<JsonElement>();
            int sessionId = bookJson.GetProperty("sessionId").GetInt32();

            // -------------------------------------------------------------
            // Step 9: Simulate Session Completion & Tutor Records Progress Result
            // -------------------------------------------------------------
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TutorMatchingPlatformDbContext>();
                var session = await db.Sessions.FindAsync(sessionId);
                if (session != null)
                {
                    session.Status = SessionStatus.Completed; // Mark completed for feedback & progress
                    await db.SaveChangesAsync();
                }
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tutorToken);
            var recordResultResponse = await _client.PostAsJsonAsync("/api/progress/record-result", new
            {
                sessionId = sessionId,
                score = 9.5m,
                tutorComment = "Excellent performance in math!",
                goalCompletionPercentage = 100
            });
            recordResultResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // -------------------------------------------------------------
            // Step 10: Student Rates Session
            // -------------------------------------------------------------
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);
            var rateResponse = await _client.PostAsJsonAsync("/api/Feedback/rate", new
            {
                sessionId = sessionId,
                rating = 5,
                comment = "Outstanding tutor, highly recommended!"
            });
            rateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify Reputation Score updated on Tutor Profile
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TutorMatchingPlatformDbContext>();
                var tutorUser = await db.Users.Include(u => u.TutorProfile).FirstOrDefaultAsync(u => u.Id == tutorUserId);
                tutorUser!.TutorProfile!.ReputationScore.Should().Be(5.0);
            }
        }
    }
}
