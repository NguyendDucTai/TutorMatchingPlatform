using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TutorMatchingPlatform.API.Common
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
                    validationEx.Errors.Select(x => x.ErrorMessage).ToArray()
                ),
                UnauthorizedAccessException unauthEx => (
                    StatusCodes.Status401Unauthorized,
                    new[] { unauthEx.Message }
                ),
                KeyNotFoundException notFoundEx => (
                    StatusCodes.Status404NotFound,
                    new[] { notFoundEx.Message }
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
