using System;
using System.Collections.Generic;

namespace TutorPlatform.Infrastructure.Models
{
    public class SubjectDataModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<TutorSubjectDataModel> TutorSubjects { get; set; } = new List<TutorSubjectDataModel>();
        public ICollection<BookingDataModel> Bookings { get; set; } = new List<BookingDataModel>();
    }
}
