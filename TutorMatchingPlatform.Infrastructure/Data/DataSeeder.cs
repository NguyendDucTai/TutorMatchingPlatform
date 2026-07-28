using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces.Authentication;
using TutorMatchingPlatform.Domain.Entities;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(TutorMatchingPlatformDbContext context, IPasswordHasher passwordHasher)
        {
            await context.Database.MigrateAsync();

            // ─── 1. Admin ──────────────────────────────────────────────────
            if (!await context.Users.AnyAsync(u => u.Role == UserRole.Administrator))
            {
                var adminUser = new User
                {
                    FullName = "System Administrator",
                    Email = "admin@tutormatching.com",
                    PasswordHash = passwordHasher.HashPassword("Admin@123"),
                    Role = UserRole.Administrator,
                    IsSuspended = false,
                    CreditBalance = 0
                };
                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }

            // ─── 2. Subjects ───────────────────────────────────────────────
            if (!await context.Subjects.AnyAsync())
            {
                var subjects = new List<Subject>
                {
                    new Subject { Name = "Toán học",        Description = "Đại số, Giải tích, Hình học và Xác suất thống kê.", IsActive = true },
                    new Subject { Name = "Vật lý",          Description = "Cơ học, Điện từ, Quang học và Vật lý hiện đại.",    IsActive = true },
                    new Subject { Name = "Hóa học",         Description = "Hóa hữu cơ, Hóa vô cơ và Hóa phân tích.",          IsActive = true },
                    new Subject { Name = "Tiếng Anh",       Description = "Ngữ pháp, Từ vựng, IELTS và TOEIC.",                IsActive = true },
                    new Subject { Name = "Lập trình C#",    Description = "C# cơ bản đến nâng cao, .NET, ASP.NET Core.",       IsActive = true },
                    new Subject { Name = "Python",          Description = "Python cơ bản, Data Science và Machine Learning.",   IsActive = true },
                    new Subject { Name = "Văn học",         Description = "Phân tích tác phẩm, Kỹ năng viết luận.",            IsActive = true },
                    new Subject { Name = "Lịch sử",         Description = "Lịch sử Việt Nam và Lịch sử thế giới.",             IsActive = true },
                    new Subject { Name = "Địa lý",          Description = "Địa lý tự nhiên và Địa lý kinh tế - xã hội.",       IsActive = true },
                    new Subject { Name = "Sinh học",        Description = "Sinh học tế bào, Di truyền học và Sinh thái học.",   IsActive = true },
                };
                await context.Subjects.AddRangeAsync(subjects);
                await context.SaveChangesAsync();
            }

            // ─── 3. Students ───────────────────────────────────────────────
            if (!await context.Users.AnyAsync(u => u.Role == UserRole.Student))
            {
                var students = new List<User>
                {
                    new User { FullName = "Nguyễn Văn An",   Email = "student1@test.com", PasswordHash = passwordHasher.HashPassword("Student@123"), Role = UserRole.Student, CreditBalance = 500 },
                    new User { FullName = "Trần Thị Bích",   Email = "student2@test.com", PasswordHash = passwordHasher.HashPassword("Student@123"), Role = UserRole.Student, CreditBalance = 300 },
                    new User { FullName = "Lê Hoàng Minh",   Email = "student3@test.com", PasswordHash = passwordHasher.HashPassword("Student@123"), Role = UserRole.Student, CreditBalance = 750 },
                };
                await context.Users.AddRangeAsync(students);
                await context.SaveChangesAsync();

                // StudentProfiles
                var subjectIds = await context.Subjects.Select(s => s.Id).Take(3).ToListAsync();
                var studentUsers = await context.Users.Where(u => u.Role == UserRole.Student).ToListAsync();

                var studentProfiles = studentUsers.Select(u => new StudentProfile
                {
                    UserId = u.Id,
                    StudyGoals = "Cải thiện kết quả học tập và chuẩn bị cho kỳ thi.",
                    TargetSubjectsJson = JsonSerializer.Serialize(subjectIds)
                }).ToList();

                await context.StudentProfiles.AddRangeAsync(studentProfiles);
                await context.SaveChangesAsync();
            }

            // ─── 4. Tutors ─────────────────────────────────────────────────
            if (!await context.Users.AnyAsync(u => u.Role == UserRole.Tutor))
            {
                // FreeSchedules: mỗi tutor dạy Thứ 2–6, 08:00–17:00
                var defaultSchedule = BuildDefaultScheduleJson();

                var tutorData = new[]
                {
                    new { FullName = "Phạm Thị Lan",    Email = "tutor1@test.com", Bio = "Giáo viên Toán và Vật lý với 8 năm kinh nghiệm. Tốt nghiệp Đại học Bách Khoa Hà Nội.",                     Rate = 150_000m, SubjectNames = new[] { "Toán học", "Vật lý" },   Score = 4.8, CreditBalance = 1200m },
                    new { FullName = "Nguyễn Minh Đức", Email = "tutor2@test.com", Bio = "Chuyên gia lập trình C# và Python. 5 năm kinh nghiệm phát triển phần mềm và giảng dạy.",                   Rate = 200_000m, SubjectNames = new[] { "Lập trình C#", "Python" }, Score = 4.9, CreditBalance = 2000m },
                    new { FullName = "Lê Thị Hoa",      Email = "tutor3@test.com", Bio = "Thạc sĩ Ngôn ngữ Anh, chuyên gia luyện IELTS. Học viên đạt band 7.5+ sau 3 tháng.",                        Rate = 180_000m, SubjectNames = new[] { "Tiếng Anh" },             Score = 4.7, CreditBalance = 900m  },
                    new { FullName = "Trần Quốc Hùng",  Email = "tutor4@test.com", Bio = "Giảng viên Hóa học, đã hướng dẫn hơn 200 học sinh thi đại học thành công.",                                 Rate = 120_000m, SubjectNames = new[] { "Hóa học", "Sinh học" },   Score = 4.6, CreditBalance = 600m  },
                    new { FullName = "Vũ Thị Mai",      Email = "tutor5@test.com", Bio = "Giáo viên Văn học và Lịch sử. Phương pháp dạy sáng tạo, giúp học sinh yêu thích môn học.",                  Rate = 100_000m, SubjectNames = new[] { "Văn học", "Lịch sử" },   Score = 4.5, CreditBalance = 450m  },
                    new { FullName = "Đặng Văn Nam",    Email = "tutor6@test.com", Bio = "Kỹ sư phần mềm tại Google, có niềm đam mê chia sẻ kiến thức về Python và Data Science.",                    Rate = 250_000m, SubjectNames = new[] { "Python", "Toán học" },   Score = 5.0, CreditBalance = 3000m },
                    new { FullName = "Hoàng Thị Yến",   Email = "tutor7@test.com", Bio = "Chuyên gia Địa lý và Sinh học. 10 năm kinh nghiệm ôn thi THPT Quốc gia.",                                   Rate = 110_000m, SubjectNames = new[] { "Địa lý", "Sinh học" },   Score = 4.4, CreditBalance = 380m  },
                    new { FullName = "Bùi Trọng Nghĩa", Email = "tutor8@test.com", Bio = "Tiến sĩ Toán học ứng dụng, giảng viên đại học với 12 năm kinh nghiệm nghiên cứu và giảng dạy.",             Rate = 300_000m, SubjectNames = new[] { "Toán học", "Vật lý" },   Score = 4.9, CreditBalance = 2500m },
                };

                // Tạo User trước
                var tutorUsers = tutorData.Select(t => new User
                {
                    FullName = t.FullName,
                    Email = t.Email,
                    PasswordHash = passwordHasher.HashPassword("Tutor@123"),
                    Role = UserRole.Tutor,
                    CreditBalance = t.CreditBalance,
                    IsSuspended = false
                }).ToList();

                await context.Users.AddRangeAsync(tutorUsers);
                await context.SaveChangesAsync();

                // Lấy lại danh sách subjects
                var allSubjects = await context.Subjects.ToListAsync();

                // Tạo TutorProfile
                var tutorProfiles = new List<TutorProfile>();
                for (int i = 0; i < tutorData.Length; i++)
                {
                    var data = tutorData[i];
                    var user = tutorUsers[i];

                    var subjectIds = allSubjects
                        .Where(s => data.SubjectNames.Contains(s.Name))
                        .Select(s => s.Id)
                        .ToList();

                    // SubjectsJson: list of { subjectId, hourlyRate }
                    var subjectsJson = JsonSerializer.Serialize(
                        subjectIds.Select(id => new { subjectId = id, hourlyRate = data.Rate })
                    );

                    tutorProfiles.Add(new TutorProfile
                    {
                        UserId = user.Id,
                        Bio = data.Bio,
                        Qualifications = $"Bằng cấp và chứng chỉ của {user.FullName}",
                        Status = ProfileStatus.Approved,
                        FreeSchedulesJson = defaultSchedule,
                        TimezoneOffset = "+07:00",
                        SubjectsJson = subjectsJson,
                        ReputationScore = data.Score
                    });
                }

                await context.TutorProfiles.AddRangeAsync(tutorProfiles);
                await context.SaveChangesAsync();
            }

            // ─── 5. Sessions & Feedbacks ───────────────────────────────────
            if (!await context.Sessions.AnyAsync())
            {
                var tutorProfiles  = await context.TutorProfiles.Include(tp => tp.User).ToListAsync();
                var studentProfiles = await context.StudentProfiles.Include(sp => sp.User).ToListAsync();
                var allSubjects     = await context.Subjects.ToListAsync();

                var sessions = new List<Session>();
                var feedbacks = new List<Feedback>();
                var creditTransactions = new List<CreditTransaction>();

                var rng = new Random(42);
                var now = DateTime.UtcNow;

                // Tạo 12 completed sessions trong 30 ngày qua
                for (int i = 0; i < 12; i++)
                {
                    var tutorProfile  = tutorProfiles[i % tutorProfiles.Count];
                    var studentProfile = studentProfiles[i % studentProfiles.Count];
                    var subject        = allSubjects[i % allSubjects.Count];

                    var daysAgo   = rng.Next(2, 30);
                    var startTime = now.AddDays(-daysAgo).Date.AddHours(8 + rng.Next(0, 8));
                    var endTime   = startTime.AddHours(1.5);

                    var session = new Session
                    {
                        TutorId   = tutorProfile.Id,
                        StudentId = studentProfile.Id,
                        SubjectId = subject.Id,
                        StartTime = startTime,
                        EndTime   = endTime,
                        Status    = SessionStatus.Completed,
                        Score     = Math.Round(rng.NextDouble() * 3 + 7, 1), // 7.0–10.0
                        TutorComment = "Học sinh tiến bộ tốt, cần luyện thêm bài tập ứng dụng.",
                        GoalCompletionPercentage = rng.Next(60, 100),
                        MeetingLink = "https://meet.google.com/abc-defg-hij",
                        ReminderSent = true
                    };
                    sessions.Add(session);
                }

                // Tạo 5 upcoming confirmed sessions
                for (int i = 0; i < 5; i++)
                {
                    var tutorProfile  = tutorProfiles[i % tutorProfiles.Count];
                    var studentProfile = studentProfiles[(i + 1) % studentProfiles.Count];
                    var subject        = allSubjects[(i + 2) % allSubjects.Count];

                    var daysAhead = rng.Next(1, 10);
                    var startTime = now.AddDays(daysAhead).Date.AddHours(9 + rng.Next(0, 6));
                    var endTime   = startTime.AddHours(1.5);

                    sessions.Add(new Session
                    {
                        TutorId   = tutorProfile.Id,
                        StudentId = studentProfile.Id,
                        SubjectId = subject.Id,
                        StartTime = startTime,
                        EndTime   = endTime,
                        Status    = SessionStatus.Confirmed,
                        MeetingLink = "https://meet.google.com/xyz-uvwx-yz1",
                        ReminderSent = false
                    });
                }

                // Tạo 3 pending sessions
                for (int i = 0; i < 3; i++)
                {
                    var tutorProfile  = tutorProfiles[(i + 3) % tutorProfiles.Count];
                    var studentProfile = studentProfiles[i % studentProfiles.Count];
                    var subject        = allSubjects[(i + 4) % allSubjects.Count];

                    var daysAhead = rng.Next(5, 15);
                    var startTime = now.AddDays(daysAhead).Date.AddHours(10 + rng.Next(0, 4));
                    var endTime   = startTime.AddHours(1.5);

                    sessions.Add(new Session
                    {
                        TutorId   = tutorProfile.Id,
                        StudentId = studentProfile.Id,
                        SubjectId = subject.Id,
                        StartTime = startTime,
                        EndTime   = endTime,
                        Status    = SessionStatus.Pending,
                        ReminderSent = false
                    });
                }

                await context.Sessions.AddRangeAsync(sessions);
                await context.SaveChangesAsync();

                // ── Feedbacks cho completed sessions ──
                var completedSessions = sessions.Where(s => s.Status == SessionStatus.Completed).ToList();
                foreach (var session in completedSessions)
                {
                    var tutorProfile  = tutorProfiles.First(tp => tp.Id == session.TutorId);
                    var studentProfile = studentProfiles.First(sp => sp.Id == session.StudentId);

                    // Student đánh giá tutor
                    feedbacks.Add(new Feedback
                    {
                        SessionId  = session.Id,
                        SenderId   = studentProfile.UserId,
                        ReceiverId = tutorProfile.UserId,
                        Rating     = rng.Next(4, 6),
                        Comment    = GetRandomStudentComment(rng)
                    });

                    // Tutor đánh giá student (50% sessions)
                    if (rng.Next(2) == 0)
                    {
                        feedbacks.Add(new Feedback
                        {
                            SessionId  = session.Id,
                            SenderId   = tutorProfile.UserId,
                            ReceiverId = studentProfile.UserId,
                            Rating     = rng.Next(3, 6),
                            Comment    = GetRandomTutorComment(rng)
                        });
                    }

                    // Credit transaction: student trả tiền
                    decimal fee = 150_000m;
                    creditTransactions.Add(new CreditTransaction
                    {
                        UserId      = studentProfile.UserId,
                        Amount      = -fee,
                        Type        = CreditTransactionType.SessionFee,
                        ReferenceId = session.Id.ToString(),
                        Description = $"Phí buổi học session #{session.Id}"
                    });

                    // Credit transaction: tutor nhận tiền
                    creditTransactions.Add(new CreditTransaction
                    {
                        UserId      = tutorProfile.UserId,
                        Amount      = fee * 0.9m, // platform giữ 10%
                        Type        = CreditTransactionType.SessionFee,
                        ReferenceId = session.Id.ToString(),
                        Description = $"Thanh toán từ session #{session.Id}"
                    });
                }

                await context.Feedbacks.AddRangeAsync(feedbacks);
                await context.CreditTransactions.AddRangeAsync(creditTransactions);
                await context.SaveChangesAsync();
            }

            // ─── 6. Một CreditRequest Pending để Admin test duyệt ──────────
            if (!await context.CreditRequests.AnyAsync())
            {
                var studentUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Student);
                if (studentUser != null)
                {
                    await context.CreditRequests.AddAsync(new CreditRequest
                    {
                        UserId = studentUser.Id,
                        Amount = 500_000m,
                        Status = CreditRequestStatus.Pending,
                        Note   = "Nạp tiền để học thêm khoá luyện thi IELTS"
                    });
                    await context.SaveChangesAsync();
                }
            }
        }

        // ─── Helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Tạo lịch rảnh mặc định: Thứ 2–6, 08:00–17:00, múi giờ +07:00
        /// </summary>
        private static string BuildDefaultScheduleJson()
        {
            // Format khớp với SearchTutorsQueryHandler (array of { dayOfWeek, startHour, endHour })
            var schedule = new[]
            {
                new { dayOfWeek = 1, startHour = 8, endHour = 17 }, // Monday
                new { dayOfWeek = 2, startHour = 8, endHour = 17 }, // Tuesday
                new { dayOfWeek = 3, startHour = 8, endHour = 17 }, // Wednesday
                new { dayOfWeek = 4, startHour = 8, endHour = 17 }, // Thursday
                new { dayOfWeek = 5, startHour = 8, endHour = 17 }, // Friday
            };
            return JsonSerializer.Serialize(schedule);
        }

        private static readonly string[] StudentComments =
        {
            "Thầy/Cô giải thích rất rõ ràng, dễ hiểu. Mình nắm được bài nhanh hơn hẳn.",
            "Phương pháp giảng dạy rất hay, sẽ đặt lịch tiếp.",
            "Rất tận tâm và kiên nhẫn. Cảm ơn thầy/cô rất nhiều!",
            "Nội dung bài học bám sát chương trình, đúng trọng tâm cần ôn.",
            "Thầy/Cô rất nhiệt tình và có kinh nghiệm thực tế phong phú.",
        };

        private static readonly string[] TutorComments =
        {
            "Học sinh chăm chỉ và có tiến bộ rõ rệt.",
            "Em cần luyện tập thêm phần bài tập ứng dụng.",
            "Em học tốt, nắm bắt kiến thức nhanh.",
            "Em cần ôn lại lý thuyết cơ bản trước khi làm bài nâng cao.",
        };

        private static string GetRandomStudentComment(Random rng)
            => StudentComments[rng.Next(StudentComments.Length)];

        private static string GetRandomTutorComment(Random rng)
            => TutorComments[rng.Next(TutorComments.Length)];
    }
}
