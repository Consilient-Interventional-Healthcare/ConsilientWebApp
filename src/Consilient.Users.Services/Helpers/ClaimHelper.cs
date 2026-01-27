using Consilient.Data.Entities.Identity;
using Consilient.Users.Contracts;
using System.Security.Claims;

namespace Consilient.Users.Services.Helpers;

/// <summary>
/// Helper class for creating and managing user claims.
/// </summary>
internal static class ClaimHelper
{
    /// <summary>
    /// Creates System.Security.Claims.Claim array by merging database claims with basic user claims.
    /// Ensures that basic claims (NameIdentifier, Name, Email) are always present.
    /// </summary>
    /// <param name="user">The user to create claims for.</param>
    /// <param name="dbClaims">Claims retrieved from the database.</param>
    /// <returns>Array of Claim objects for JWT token generation.</returns>
    public static Claim[] MergeClaims(User user, IEnumerable<Claim> dbClaims)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(dbClaims);

        var claimsList = dbClaims.ToList();
        var basicClaimTypes = GetBasicClaimTypes(user);

        // Add basic claims if they don't exist in database claims
        foreach (var (type, value) in basicClaimTypes)
        {
            if (!claimsList.Exists(c => c.Type == type))
            {
                claimsList.Add(new Claim(type, value));
            }
        }

        return [.. claimsList];
    }

    /// <summary>
    /// Converts Claim collection to ClaimDto collection.
    /// </summary>
    /// <param name="claims">Collection of Claim objects.</param>
    /// <returns>Collection of ClaimDto objects.</returns>
    public static IEnumerable<ClaimDto> MapToDto(IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);
        return claims.Select(c => new ClaimDto(c.Type, c.Value));
    }

    /// <summary>
    /// Creates a CurrentUserDto from claims, extracting common properties and preserving additional claims.
    /// </summary>
    /// <param name="claims">Collection of Claim objects.</param>
    /// <returns>CurrentUserDto with extracted properties and additional claims.</returns>
    public static CurrentUserDto MapToCurrentUserDto(IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        var claimsList = claims.ToList();
        var knownClaimTypes = new HashSet<string>
        {
            ClaimTypes.NameIdentifier,
            ClaimTypes.Name,
            ClaimTypes.Email
        };

        return new CurrentUserDto(
            Id: claimsList.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            UserName: claimsList.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty,
            Email: claimsList.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty,
            AdditionalClaims: claimsList
                .Where(c => !knownClaimTypes.Contains(c.Type))
                .Select(c => new ClaimDto(c.Type, c.Value))
        );
    }

    /// <summary>
    /// Gets the basic claim types and their values for a user.
    /// </summary>
    /// <param name="user">The user to create claims for.</param>
    /// <returns>Dictionary of claim types and values.</returns>
    private static Dictionary<string, string> GetBasicClaimTypes(User user) =>
        new()
        {
            [ClaimTypes.NameIdentifier] = user.Id.ToString(),
            [ClaimTypes.Name] = user.UserName ?? string.Empty,
            [ClaimTypes.Email] = user.Email ?? string.Empty
        };
}