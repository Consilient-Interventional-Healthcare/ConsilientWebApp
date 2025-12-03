using Consilient.Api.Configuration;
using Consilient.Api.Infra;
using Consilient.Users.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Consilient.Api.Controllers
{
    public record AuthenticateUserResult(bool Succeeded, IEnumerable<string>? Errors = null, IEnumerable<ClaimDto>? UserClaims = null);

    [Route("[controller]")]
    [ApiController]
    public class AuthController(IUserService _userService, ApplicationSettings applicationSettings) : ControllerBase
    {
        [HttpPost("authenticate")]
        [EnableRateLimiting("AuthenticatePolicy")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateUserRequest request)
        {
            if (request is null)
            {
                return BadRequest("Request body is required.");
            }

            var result = await _userService.AuthenticateUserAsync(request);
            if (result.Succeeded)
            {
                Response.AppendAuthTokenCookie(result.Token!, applicationSettings.Authentication.Jwt.ExpiryMinutes);
                return Ok(new AuthenticateUserResult(true, null, result.Claims));
            }

            return Unauthorized(new AuthenticateUserResult(false, result.Errors, null));
        }

        [HttpGet("claims")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userName = User.Identity!.Name!;
            var result = await _userService.GetClaimsAsync(userName);
            return Ok(result);
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

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            Response.DeleteAuthTokenCookie();
            return Ok();
        }
    }
}
