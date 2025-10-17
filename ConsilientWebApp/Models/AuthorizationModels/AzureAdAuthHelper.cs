using System.Security.Claims;
using System.Text.Json;

namespace ConsilientWebApp.Models.AuthorizationModels
{
    public static class AzureAdAuthHelper
    {
        public static ClaimsPrincipal GetUser(HttpContext context)
        {
            var header = context.Request.Headers["X-MS-CLIENT-PRINCIPAL"].FirstOrDefault();
            if (string.IsNullOrEmpty(header))
                return new ClaimsPrincipal();

            var decoded = Convert.FromBase64String(header);
            var json = System.Text.Encoding.UTF8.GetString(decoded);

            var clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var claims = clientPrincipal?.Claims?.Select(c => new Claim(c.Type, c.Value)) ?? Enumerable.Empty<Claim>();
            return new ClaimsPrincipal(new ClaimsIdentity(claims, clientPrincipal?.AuthenticationType));
        }
    }
}
