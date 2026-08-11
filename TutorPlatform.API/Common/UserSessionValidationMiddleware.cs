using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TutorPlatform.API.Common
{
    public class UserSessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public UserSessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? context.User.FindFirstValue("nameid")
                             ?? context.User.FindFirstValue("sub");

                if (Guid.TryParse(userIdStr, out var userId))
                {
                    string authHeader = context.Request.Headers["Authorization"].ToString();
                    string token = null;
                    if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        token = authHeader.Substring("Bearer ".Length).Trim();
                    }
                    else if (context.Request.Query.TryGetValue("access_token", out var queryToken))
                    {
                        token = queryToken.ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(token) && !UserSessionManager.IsTokenValid(userId, token))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var errorResponse = ApiResponse<object>.Error(401, "Tài khoản của bạn đã được đăng nhập ở nơi khác.");
                        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
