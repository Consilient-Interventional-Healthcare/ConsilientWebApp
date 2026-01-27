using System.Security.Claims;

namespace Consilient.Users.Contracts;

public interface ITokenGenerator
{
    string GenerateToken(IEnumerable<Claim> claims);
}