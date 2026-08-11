using System;
using System.Collections.Concurrent;

namespace TutorPlatform.API.Common
{
    public static class UserSessionManager
    {
        private static readonly ConcurrentDictionary<Guid, string> _activeTokens = new ConcurrentDictionary<Guid, string>();

        public static void RegisterSession(Guid userId, string token)
        {
            if (userId != Guid.Empty && !string.IsNullOrWhiteSpace(token))
            {
                _activeTokens[userId] = token;
            }
        }

        public static bool IsTokenValid(Guid userId, string token)
        {
            if (userId == Guid.Empty) return true;
            if (!_activeTokens.TryGetValue(userId, out var activeToken))
            {
                return true; // Allow if no active session registered yet
            }
            return string.Equals(activeToken, token, StringComparison.Ordinal);
        }
    }
}
