using MediatR;
using System;
using System.Collections.Generic;
using TutorMatchingPlatform.Application.Sessions.Queries.GetSessionById;

namespace TutorMatchingPlatform.Application.Sessions.Queries.GetMySessions
{
    public class GetMySessionsQuery : IRequest<List<SessionDto>>
    {
        public int UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
