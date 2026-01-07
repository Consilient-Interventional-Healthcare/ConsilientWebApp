using Consilient.Infrastructure.Logging.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly LoggingConfiguration? _loggingConfiguration;
        private readonly IConfiguration _configuration;

        public DiagnosticsController(
            LoggingConfiguration? loggingConfiguration,
            IConfiguration configuration)
        {
            _loggingConfiguration = loggingConfiguration;
            _configuration = configuration;
        }

        [HttpGet("loki-config")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or add auth
        public IActionResult GetLokiConfig()
        {
            return Ok(new
            {
                LokiUrlFromConfig = _loggingConfiguration?.GrafanaLoki?.Url,
                LokiPushEndpoint = _loggingConfiguration?.GrafanaLoki?.PushEndpoint,
                ConstructedReadyUrl = !string.IsNullOrEmpty(_loggingConfiguration?.GrafanaLoki?.Url)
                    ? $"{_loggingConfiguration.GrafanaLoki.Url.TrimEnd('/')}/ready"
                    : "N/A"
            });
        }

        [HttpGet("test-loki")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or add auth
        public async Task<IActionResult> TestLokiConnection()
        {
            var lokiUrl = _loggingConfiguration?.GrafanaLoki?.Url;
            if (string.IsNullOrEmpty(lokiUrl))
            {
                return BadRequest("Loki URL not configured");
            }

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var readyUrl = $"{lokiUrl.TrimEnd('/')}/ready";

            try
            {
                var response = await client.GetAsync(readyUrl);
                var body = await response.Content.ReadAsStringAsync();

                return Ok(new
                {
                    Url = readyUrl,
                    StatusCode = (int)response.StatusCode,
                    Body = body
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Url = readyUrl,
                    Error = ex.Message,
                    ExceptionType = ex.GetType().Name
                });
            }
        }

        [HttpGet("app-config")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or restrict with auth
        public IActionResult GetAppConfiguration([FromQuery] string? prefix = null)
        {
            var configKeys = new Dictionary<string, object?>();

            // Get all configuration keys (or filter by prefix)
            var allConfig = _configuration.AsEnumerable()
                .Where(kvp => !string.IsNullOrEmpty(kvp.Key))
                .Where(kvp => string.IsNullOrEmpty(prefix) || kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(kvp => kvp.Key);

            foreach (var kvp in allConfig)
            {
                // Mask sensitive values
                var isSensitive = kvp.Key.Contains("Secret", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                                  kvp.Key.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase);

                configKeys[kvp.Key] = isSensitive ? "***MASKED***" : kvp.Value;
            }

            return Ok(new
            {
                TotalKeys = configKeys.Count,
                FilterPrefix = prefix,
                Configuration = configKeys
            });
        }

        [HttpGet("app-config/verify")]
        [AllowAnonymous]  // TEMPORARY - Remove in production or restrict with auth
        public IActionResult VerifyOAuthConfiguration()
        {
            // Verify specific OAuth keys are loaded with expected values
            var oauthSection = "ApplicationSettings:Authentication:UserService:OAuth";

            var result = new
            {
                OAuthEnabled = _configuration[$"{oauthSection}:Enabled"],
                OAuthProviderName = _configuration[$"{oauthSection}:ProviderName"],
                OAuthClientId = !string.IsNullOrEmpty(_configuration[$"{oauthSection}:ClientId"]) ? "SET" : "NOT SET",
                OAuthClientSecret = !string.IsNullOrEmpty(_configuration[$"{oauthSection}:ClientSecret"]) ? "SET" : "NOT SET",
                OAuthTenantId = !string.IsNullOrEmpty(_configuration[$"{oauthSection}:TenantId"]) ? "SET" : "NOT SET",
                OAuthAuthority = _configuration[$"{oauthSection}:Authority"],
                OAuthScopes = _configuration[$"{oauthSection}:Scopes"],

                // Show config source indicator
                AppConfigurationEndpoint = _configuration["AppConfiguration:Endpoint"] ?? "NOT SET",
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"]
            };

            return Ok(result);
        }
    }
}
