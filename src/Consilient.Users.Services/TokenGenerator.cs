using Consilient.Users.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Consilient.Users.Services;

public class TokenGenerator(IOptions<UserServiceOptions> userServiceOptions) : ITokenGenerator
{
    private readonly TokenGeneratorOptions _jwtOptions = userServiceOptions.Value?.Jwt
        ?? throw new InvalidOperationException(
            "JWT configuration is missing. Please ensure the JWT section is properly configured in application settings.");

    public string GenerateToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
