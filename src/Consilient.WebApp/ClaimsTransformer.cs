using Consilient.WebApp.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Consilient.WebApp
{
    public class ClaimsTransformer(ConsilientContext context) : IClaimsTransformation
    {
        private readonly ConsilientContext _context = context;

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var email = principal.FindFirst(ClaimTypes.Email)?.Value
                     ?? principal.FindFirst("preferred_username")?.Value;

            email = email?.ToLower();

            if (!string.IsNullOrEmpty(email))
            {
                var isAdmin = await _context.Employees
                    .AnyAsync(u => u.Email == email && u.IsAdministrator);

                if (isAdmin)
                {
                    var identity = (ClaimsIdentity)principal.Identity!;
                    if (!principal.IsInRole("Administrator"))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
                    }
                }

                var canApproveVisits = await _context.Employees
                    .AnyAsync(u => u.Email == email && u.CanApproveVisits);

                if (canApproveVisits)
                {
                    var identity = (ClaimsIdentity)principal.Identity!;
                    if (!principal.IsInRole("CanApproveVisits"))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "CanApproveVisits"));
                    }
                }
            }

            return principal;
        }
    }

}
