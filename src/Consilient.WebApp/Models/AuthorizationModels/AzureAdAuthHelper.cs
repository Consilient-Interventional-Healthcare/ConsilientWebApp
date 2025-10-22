using System.Security.Claims;
using System.Text.Json;

namespace Consilient.WebApp.Models.AuthorizationModels
{
    public static class AzureAdAuthHelper
    {
        // Cache the JsonSerializerOptions instance
        private static readonly JsonSerializerOptions CachedJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static ClaimsPrincipal GetUser(HttpContext context)
        {
            var header = context.Request.Headers["X-MS-CLIENT-PRINCIPAL"].FirstOrDefault();
            if (string.IsNullOrEmpty(header))
            {
                return new ClaimsPrincipal();
            }

            var decoded = Convert.FromBase64String(header);
            var json = System.Text.Encoding.UTF8.GetString(decoded);

            var clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(json, CachedJsonOptions);

            var claims = clientPrincipal?.Claims?.Select(c => new Claim(c.Type, c.Value)) ?? [];
            return new ClaimsPrincipal(new ClaimsIdentity(claims, clientPrincipal?.AuthenticationType));
        }
    }
}
