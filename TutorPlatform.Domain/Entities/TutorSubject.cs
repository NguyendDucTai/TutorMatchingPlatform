using System;
using TutorPlatform.Domain.Common;
using TutorPlatform.Domain.Enums;

namespace TutorPlatform.Domain.Entities
{
    public class TutorSubject : BaseEntity
    {
        public Guid TutorProfileId { get; private set; }
        public Guid SubjectId { get; private set; }
        public ProficiencyLevel ProficiencyLevel { get; private set; }
        public decimal HourlyCredits { get; private set; }

        private TutorSubject() { } // EF Core

        public TutorSubject(Guid tutorProfileId, Guid subjectId, ProficiencyLevel proficiencyLevel, decimal hourlyCredits)
        {
            TutorProfileId = tutorProfileId;
            SubjectId = subjectId;
            ProficiencyLevel = proficiencyLevel;
            HourlyCredits = hourlyCredits;
        }

        public void UpdateProficiencyLevel(ProficiencyLevel level)
        {
            ProficiencyLevel = level;
            MarkUpdated();
        }

        public void UpdatePricing(decimal hourlyCredits)
        {
            HourlyCredits = hourlyCredits;
            MarkUpdated();
        }
    }
}
