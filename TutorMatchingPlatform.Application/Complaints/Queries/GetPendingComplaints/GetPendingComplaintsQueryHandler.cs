using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TutorMatchingPlatform.Application.Interfaces;
using TutorMatchingPlatform.Domain.Enums;

namespace TutorMatchingPlatform.Application.Complaints.Queries.GetPendingComplaints
{
    public class GetPendingComplaintsQueryHandler : IRequestHandler<GetPendingComplaintsQuery, List<ComplaintDto>>
    {
        private readonly IAppDbContext _context;

        public GetPendingComplaintsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ComplaintDto>> Handle(GetPendingComplaintsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Complaints
                .Where(c => c.Status == ComplaintStatus.Pending)
                .Include(c => c.Reporter)
                .Include(c => c.ReportedUser)
                .Select(c => new ComplaintDto
                {
                    Id = c.Id,
                    ReporterId = c.ReporterId,
                    ReporterName = c.Reporter != null ? c.Reporter.FullName : "System",
                    ReportedUserId = c.ReportedUserId,
                    ReportedUserName = c.ReportedUser.FullName,
                    SessionId = c.SessionId,
                    Type = c.Type,
                    Source = c.Source,
                    Description = c.Description,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}
