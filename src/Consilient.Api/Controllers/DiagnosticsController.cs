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
    }
}
