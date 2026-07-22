using System;
using MediatR;

namespace TutorMatchingPlatform.Application.Sessions.Commands.BookSession
{
    public class BookSessionCommand : IRequest<BookSessionResult>
    {
        public int StudentId { get; set; } // Sourced from logged-in user claims
        public int TutorId { get; set; }
        public int SubjectId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
