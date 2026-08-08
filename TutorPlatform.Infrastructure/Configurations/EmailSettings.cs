namespace TutorPlatform.Infrastructure.Configurations
{
    public class EmailSettings
    {
        public string FromEmail { get; set; } = "nguyenductai08102004ok@gmail.com";
        public string FromName { get; set; } = "TutorMatching Platform";
        
        // Backward compatibility alias
        public string SenderEmail
        {
            get => FromEmail;
            set => FromEmail = value;
        }
        public string SenderName
        {
            get => FromName;
            set => FromName = value;
        }
    }
}
