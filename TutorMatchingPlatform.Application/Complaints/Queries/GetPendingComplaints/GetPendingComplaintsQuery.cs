using System.Collections.Generic;
using MediatR;

namespace TutorMatchingPlatform.Application.Complaints.Queries.GetPendingComplaints
{
    public class GetPendingComplaintsQuery : IRequest<List<ComplaintDto>>
    {
    }
}
