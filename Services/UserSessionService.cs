namespace ASM_1.Services
{
    public class UserSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetOrCreateUserSessionId(string tableCode)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return tableCode;

            string sessionKey = $"UserSessionId_{tableCode}";
            var sessionId = context.Session.GetString(sessionKey);

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = $"{tableCode}_{Guid.NewGuid():N}";
                context.Session.SetString(sessionKey, sessionId);
            }
            return sessionId;
        }
    }
}
