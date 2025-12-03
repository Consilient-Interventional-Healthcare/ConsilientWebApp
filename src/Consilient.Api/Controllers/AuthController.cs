using Consilient.Users.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Consilient.Api.Controllers
{
    public record AuthenticateUserResult(bool Succeeded, IEnumerable<string>? Errors = null, IEnumerable<ClaimDto>? UserClaims = null);

    [Route("[controller]")]
    [ApiController]
    public class AuthController(IUserService _userService) : ControllerBase
    {
        const string AUTH_TOKEN = "auth_token";

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
                // Set the JWT token in an HttpOnly cookie
                Response.Cookies.Append(AUTH_TOKEN, result.Token!, new CookieOptions
                {
                    HttpOnly = true,           // Prevents JavaScript access
                    Secure = true,             // Only sent over HTTPS
                    SameSite = SameSiteMode.Strict,  // CSRF protection
                    MaxAge = TimeSpan.FromHours(1),  // Match your JWT expiration
                    Path = "/"                 // Available site-wide
                });

                // Return the user's claims in the response body. Token is stored in cookie.
                return Ok(new AuthenticateUserResult (true, null, result.Claims));
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
            Response.Cookies.Delete(AUTH_TOKEN);
            return Ok();
        }
    }
}
