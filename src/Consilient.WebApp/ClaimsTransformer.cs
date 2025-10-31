using Consilient.Api.Client;
using Consilient.Api.Client.Contracts;
using Consilient.Constants;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Consilient.WebApp
{
    public class ClaimsTransformer(IEmployeesApi employeesApi) : IClaimsTransformation
    {
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var email = principal.FindFirst(ClaimTypes.Email)?.Value
                        ?? principal.FindFirst("preferred_username")?.Value;

            email = email?.ToLower();

            if (string.IsNullOrEmpty(email))
            {
                return principal;
            }

            var employee = (await employeesApi.GetByEmailAsync(email)).Unwrap()!;

            if (employee.IsAdministrator)
            {
                AddClaim(principal, ApplicationConstants.Roles.Administrator);
            }

            if (employee.CanApproveVisits)
            {
                AddClaim(principal, ApplicationConstants.Permissions.CanApproveVisits);
            }

            return principal;
        }

        private static void AddClaim(ClaimsPrincipal principal, string role)
        {
            var identity = (ClaimsIdentity)principal.Identity!;
            if (!principal.IsInRole(role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }
    }

}
