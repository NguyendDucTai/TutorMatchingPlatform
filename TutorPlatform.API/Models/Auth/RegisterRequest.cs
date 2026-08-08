namespace TutorPlatform.API.Models.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int Role { get; set; } // 1: Tutor, 2: Student
    }
}
