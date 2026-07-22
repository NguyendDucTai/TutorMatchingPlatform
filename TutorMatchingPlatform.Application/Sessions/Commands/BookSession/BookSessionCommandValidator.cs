using System;
using FluentValidation;

namespace TutorMatchingPlatform.Application.Sessions.Commands.BookSession
{
    public class BookSessionCommandValidator : AbstractValidator<BookSessionCommand>
    {
        public BookSessionCommandValidator()
        {
            RuleFor(v => v.TutorId).GreaterThan(0).WithMessage("TutorId is required.");
            RuleFor(v => v.SubjectId).GreaterThan(0).WithMessage("SubjectId is required.");
            
            RuleFor(v => v.StartTime)
                .NotEmpty().WithMessage("StartTime is required.")
                .GreaterThan(DateTime.UtcNow).WithMessage("StartTime must be in the future.");

            RuleFor(v => v.EndTime)
                .NotEmpty().WithMessage("EndTime is required.")
                .GreaterThan(v => v.StartTime).WithMessage("EndTime must be after StartTime.");

            // Optional: limit session length to 4 hours maximum
            RuleFor(v => v)
                .Must(v => (v.EndTime - v.StartTime).TotalHours <= 4)
                .WithMessage("A session cannot exceed 4 hours.")
                .When(v => v.StartTime != default && v.EndTime != default && v.EndTime > v.StartTime);
        }
    }
}
