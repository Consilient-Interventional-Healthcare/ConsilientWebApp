using Consilient.Users.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IUserService _userService) : ControllerBase
    {
        [HttpPost("authenticate")]
        [EnableRateLimiting("AuthenticatePolicy")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateUserRequest request)
        {
            if (request is null)
            {
                return BadRequest("Request body is required.");
            }

            var result = await _userService.AuthenticateUserAsync(request);
            if (result.Succeeded)
            {
                return Ok(result.Token);
            }

            return Unauthorized(new { errors = result.Errors });
        }

        [HttpPost("link-external")]
        [EnableRateLimiting("LinkExternalPolicy")]
        public async Task<IActionResult> LinkExternal([FromBody] LinkExternalLoginRequest request)
        {
            if (request is null)
            {
                return BadRequest("Request body is required.");
            }

            var result = await _userService.LinkExternalLoginAsync(request);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(new { errors = result.Errors });
        }
    }
}
