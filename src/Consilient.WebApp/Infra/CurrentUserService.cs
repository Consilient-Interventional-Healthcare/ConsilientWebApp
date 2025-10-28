using System.Security.Claims;

namespace Consilient.WebApp.Infra
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public string? UserId
        {
            get
            {
                var ctx = _httpContextAccessor.HttpContext;
                if (ctx?.User?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                // Try common claim types: NameIdentifier (ClaimTypes.NameIdentifier), "sub" or "id"
                var user = ctx.User;
                var id =
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value
                    ?? user.FindFirst("id")?.Value;

                return id;
            }
        }

        public string? UserEmail
        {
            get
            {
                var ctx = _httpContextAccessor.HttpContext;
                if (ctx?.User?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }
                var user = ctx.User;
                var email =
                    user.FindFirst(ClaimTypes.Email)?.Value
                    ?? user.FindFirst("email")?.Value;
                return email;
            }
        }
    }
}