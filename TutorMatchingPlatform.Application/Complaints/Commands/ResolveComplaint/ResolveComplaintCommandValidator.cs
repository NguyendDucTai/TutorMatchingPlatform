using FluentValidation;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Complaints.Commands.ResolveComplaint
{
    public class ResolveComplaintCommandValidator : AbstractValidator<ResolveComplaintCommand>
    {
        public ResolveComplaintCommandValidator()
        {
            RuleFor(v => v.ComplaintId).GreaterThan(0);
            RuleFor(v => v.Action).IsInEnum();
            
            RuleFor(v => v.Reason)
                .NotEmpty()
                .When(v => v.Action == ComplaintAction.TemporarySuspension || v.Action == ComplaintAction.Warning)
                .WithMessage("Reason is required when issuing a Warning or Suspension.");
                
            RuleFor(v => v.SuspendDays)
                .GreaterThan(0)
                .When(v => v.Action == ComplaintAction.TemporarySuspension)
                .WithMessage("SuspendDays must be greater than 0 for TemporarySuspension.");
        }
    }
}
