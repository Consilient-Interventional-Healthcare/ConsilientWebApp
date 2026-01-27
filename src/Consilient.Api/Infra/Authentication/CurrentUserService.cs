using Consilient.Common.Contracts;
using System.Security.Claims;

namespace Consilient.Api.Infra.Authentication;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public int UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim, out var id) ? id : 0;
        }
    }

    public string? UserEmail => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
}