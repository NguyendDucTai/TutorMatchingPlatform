using System;

namespace TutorPlatform.Application.Features.Notifications.Events.Handlers
{
    public static class NotificationNameHelper
    {
        public static string FormatUserDisplayName(string fullName, string rolePrefix)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return rolePrefix;
            var trimmed = fullName.Trim();
            if (trimmed.StartsWith(rolePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }
            return $"{rolePrefix} {trimmed}";
        }
    }
}
