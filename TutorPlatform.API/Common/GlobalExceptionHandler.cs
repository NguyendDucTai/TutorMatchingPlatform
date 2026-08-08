using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TutorPlatform.Application.Common.Exceptions;

namespace TutorPlatform.API.Common
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var (statusCode, messages) = exception switch
            {
                ValidationException validationEx => (
                    StatusCodes.Status400BadRequest,
                    validationEx.Errors.SelectMany(x => x.Value).ToArray()
                ),
                NotFoundException notFoundEx => (
                    StatusCodes.Status404NotFound,
                    new[] { notFoundEx.Message }
                ),
                ForbiddenException forbiddenEx => (
                    StatusCodes.Status403Forbidden,
                    new[] { forbiddenEx.Message }
                ),
                BadRequestException badReqEx => (
                    StatusCodes.Status400BadRequest,
                    new[] { badReqEx.Message }
                ),
                ConflictException conflictEx => (
                    StatusCodes.Status409Conflict,
                    new[] { conflictEx.Message }
                ),
                ArgumentException argEx => (
                    StatusCodes.Status400BadRequest,
                    new[] { argEx.Message }
                ),
                InvalidOperationException invEx => (
                    StatusCodes.Status400BadRequest,
                    new[] { invEx.Message }
                ),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    new[] { "An unexpected error occurred." }
                )
            };

            var response = ApiResponse<object>.Error(statusCode, messages);

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
