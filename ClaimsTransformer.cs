using ConsilientWebApp.Data;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace ConsilientWebApp
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly ConsilientContext _context;

        public ClaimsTransformer(ConsilientContext context)
        {
            _context = context;
        }

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
