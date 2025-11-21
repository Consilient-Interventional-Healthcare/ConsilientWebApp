using Consilient.Users.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IUserService _userService) : ControllerBase
    {
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateUserRequest request)
        {
            if (request is null)
            {
                return BadRequest("Request body is required.");
            }

            var result = await _userService.AuthenticateUserAsync(request).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return Ok();
            }

            return Unauthorized(new { errors = result.Errors });
        }

        [HttpPost("link-external")]
        public async Task<IActionResult> LinkExternal([FromBody] LinkExternalLoginRequest request)
        {
            if (request is null)
            {
                return BadRequest("Request body is required.");
            }

            var result = await _userService.LinkExternalLoginAsync(request).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(new { errors = result.Errors });
        }
    }
}
