using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TutorMatchingPlatform.Infrastructure.Models;
using TutorMatchingPlatform.Infrastructure.Services;

namespace TutorMatchingPlatform.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static void SeedData(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. Seed Admin User
                var adminUser = context.Users.FirstOrDefault(u => u.Role == 0 || u.Email == "admin@tutorplatform.com");
                if (adminUser == null)
                {
                    var hasher = new BCryptPasswordHasher();
                    var newAdmin = new UserDataModel
                    {
                        Id = Guid.NewGuid(),
                        Email = "admin@tutorplatform.com",
                        PasswordHash = hasher.HashPassword("Admin@123456"),
                        FullName = "System Administrator",
                        Phone = "0999999999",
                        Role = 0, // Admin
                        IsActive = true,
                        CreditBalance = 999999,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.Users.Add(newAdmin);
                    context.SaveChanges();
                }

                // 2. Seed Subjects if empty
                if (!context.Subjects.Any())
                {
                    var subjects = new List<SubjectDataModel>
                    {
                        new SubjectDataModel
                        {
                            Id = Guid.NewGuid(),
                            Name = "Toán Học Cao Cấp",
                            Category = "Toán Học",
                            Description = "Giải Tích, Đại Số Tuyến Tính, Toán Cao Cấp ĐH & Luyện Thi Học Sinh Giỏi.",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new SubjectDataModel
                        {
                            Id = Guid.NewGuid(),
                            Name = "Tiếng Anh IELTS 7.5+",
                            Category = "Ngoại Ngữ",
                            Description = "Luyện thi IELTS 4 kỹ năng Listening, Speaking, Reading, Writing cấp tốc.",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new SubjectDataModel
                        {
                            Id = Guid.NewGuid(),
                            Name = "Lập Trình Web Fullstack (React & .NET)",
                            Category = "Công Nghệ Thông Tin",
                            Description = "Xây dựng Web App thực tế với ReactJS, TypeScript, C# ASP.NET Core & SQL Server.",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new SubjectDataModel
                        {
                            Id = Guid.NewGuid(),
                            Name = "Vật Lý 12 & Luyện Thi THPT",
                            Category = "Khoa Học Tự Nhiên",
                            Description = "Dao Động Cơ, Sóng Cơ, Dòng Điện Xoay Chiều & Phương Pháp Giải Nhanh Đề Thi.",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new SubjectDataModel
                        {
                            Id = Guid.NewGuid(),
                            Name = "Hóa Học 12 Bứt Phá 9+",
                            Category = "Khoa Học Tự Nhiên",
                            Description = "Hóa Hữu Cơ, Este - Lipit, Amino Axit & Bài Tập Đếm Chuẩn Cấu Trúc Đề BGD.",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new SubjectDataModel
                        {
                            Id = Guid.NewGuid(),
                            Name = "Ngữ Văn & Sáng Tạo Tác Phẩm",
                            Category = "Khoa Học Xã Hội",
                            Description = "Nghị Luận Văn Học, Nghị Luận Xã Hội & Kỹ Năng Diễn Đạt Sâu Sắc.",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    context.Subjects.AddRange(subjects);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding data: {ex.Message}");
            }
        }
    }
}
