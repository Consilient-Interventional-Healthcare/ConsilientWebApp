using Consilient.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Consilient.WebApp
{
    public class ClaimsTransformer(ConsilientDbContext context) : IClaimsTransformation
    {
        private readonly ConsilientDbContext _context = context;

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
