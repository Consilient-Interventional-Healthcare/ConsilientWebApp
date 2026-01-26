using Consilient.Api.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SettingsController(IOptions<AuthenticationOptions> authOptions) : ControllerBase
    {
        private readonly AuthenticationOptions _authOptions = authOptions.Value;

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetSettings()
        {
            var settings = new
            {
                ExternalLoginEnabled = _authOptions.UserService?.OAuth?.Enabled ?? false
            };
            return Ok(settings);
        }
    }
}
