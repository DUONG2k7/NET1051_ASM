namespace ASM_1.Services
{
    public class UserSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "UserSessionId";
        private const string CookieKey = "UserSessionId";

        public UserSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetOrCreateUserSessionId(string tableCode)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return tableCode;

            var sessionId = context.Session.GetString(SessionKey);

            // 🔹 Nếu chưa có session hoặc session khác bàn hiện tại → tạo mới
            if (string.IsNullOrEmpty(sessionId) || !sessionId.StartsWith($"{tableCode}_"))
            {
                sessionId = $"{tableCode}_{Guid.NewGuid():N}";

                context.Session.SetString(SessionKey, sessionId);
                context.Response.Cookies.Append(CookieKey, sessionId, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddHours(2),
                    HttpOnly = true,
                    IsEssential = true
                });
            }

            return sessionId;
        }
    }
}
