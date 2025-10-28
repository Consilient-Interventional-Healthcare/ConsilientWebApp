using Consilient.Api.Client;
using Consilient.Api.Client.Contracts;
using Consilient.Constants;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Consilient.WebApp
{
    public class ClaimsTransformer(IEmployeesApi employeesApi) : IClaimsTransformation
    {
        private readonly IEmployeesApi _employeesApi = employeesApi;

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var email = principal.FindFirst(ClaimTypes.Email)?.Value
                     ?? principal.FindFirst("preferred_username")?.Value;

            email = email?.ToLower();

            if (!string.IsNullOrEmpty(email))
            {
                var employee = (await _employeesApi.GetByEmailAsync(email)).Unwrap()!;
                var identity = (ClaimsIdentity)principal.Identity!;

                if (employee.IsAdministrator)
                {
                    if (!principal.IsInRole(ApplicationConstants.Roles.Administrator))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, ApplicationConstants.Roles.Administrator));
                    }
                }

                if (employee.CanApproveVisits)
                {
                    if (!principal.IsInRole(ApplicationConstants.Permissions.CanApproveVisits))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, ApplicationConstants.Permissions.CanApproveVisits));
                    }
                }
            }

            return principal;
        }
    }

}
