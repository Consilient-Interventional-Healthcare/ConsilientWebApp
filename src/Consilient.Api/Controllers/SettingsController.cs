using Consilient.Api.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SettingsController(ApplicationSettings applicationSettings) : ControllerBase
    {
        private readonly ApplicationSettings _applicationSettings = applicationSettings;

        [HttpGet]
        public IActionResult GetSettings()
        {
            var settings = new
            {
                ExternalLoginEnabled = _applicationSettings.Authentication.External?.Microsoft?.Enabled ?? false
            };
            return Ok(settings);
        }
    }
}
