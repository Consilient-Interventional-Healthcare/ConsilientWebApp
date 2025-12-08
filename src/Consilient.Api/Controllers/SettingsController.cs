using Consilient.Api.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SettingsController(ApplicationSettings applicationSettings) : ControllerBase
    {
        private readonly ApplicationSettings _applicationSettings = applicationSettings;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetSettings()
        {
            var settings = new
            {
                ExternalLoginEnabled = _applicationSettings.Authentication?.UserService?.OAuth?.Enabled ?? false
            };
            return Ok(settings);
        }
    }
}
