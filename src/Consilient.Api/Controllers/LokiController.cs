using Consilient.Api.Models;
using Consilient.Infrastructure.Logging.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LokiController(LoggingConfiguration loggingConfiguration) : ControllerBase
    {
        [HttpPost("logs")]
        public async Task<IActionResult> ForwardToLoki([FromBody] LokiPayload payload)
        {
            var lokiUrl = loggingConfiguration.GrafanaLoki.Url;
            if (string.IsNullOrEmpty(lokiUrl))
            {
                return BadRequest("Loki URL not configured");
            }

            using var client = new HttpClient()
            {
                BaseAddress = new Uri(lokiUrl)
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/loki/api/v1/push", content);
            return response.IsSuccessStatusCode ? Ok() : StatusCode((int)response.StatusCode);
        }
    }
}
