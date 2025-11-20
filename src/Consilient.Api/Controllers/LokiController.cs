using Consilient.Infrastructure.Logging.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LokiController(LoggingConfiguration loggingConfiguration, ILogger<LokiController> _logger) : ControllerBase
    {
        [HttpPost("logs")]
        public async Task<IActionResult> ForwardToLoki()
        {
            var lokiUrl = loggingConfiguration.GrafanaLoki.Url;
            if (string.IsNullOrEmpty(lokiUrl))
            {
                var message = "Loki URL is not configured.";
                _logger.LogError("{message}", message);
                return BadRequest(message);
            }

            var pushEndpoint = loggingConfiguration.GrafanaLoki.PushEndpoint;
            if (string.IsNullOrEmpty(pushEndpoint))
            {
                var message = "Loki push endpoint is not configured.";
                _logger.LogError("{message}", message);
                return BadRequest(message);
            }
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                using var client = new HttpClient()
                {
                    BaseAddress = new Uri(lokiUrl)
                };
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(pushEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Failed to forward logs to Loki. Status: {StatusCode}, Url: {LokiUrl}, Endpoint: {PushEndpoint}, Error: {ErrorContent}, RequestBody: {RequestBody}",
                        response.StatusCode,
                        lokiUrl,
                        pushEndpoint,
                        errorContent,
                        body);

                    return StatusCode((int)response.StatusCode);
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while forwarding logs to Loki at {LokiUrl}{PushEndpoint}", lokiUrl, pushEndpoint);
                return StatusCode(500, "Internal server error while forwarding logs to Loki.");
            }
            return Ok();
        }
    }
}
