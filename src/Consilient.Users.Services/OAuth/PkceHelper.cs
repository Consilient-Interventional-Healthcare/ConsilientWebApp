using System.Security.Cryptography;
using System.Text;

namespace Consilient.Users.Services.OAuth;

public static class PkceHelper
{
    /// <summary>
    /// Generates a cryptographically secure code verifier for PKCE.
    /// </summary>
    public static string GenerateCodeVerifier()
    {
        // Generate exactly the needed bytes, resulting in ~43 URL-safe base64 characters
        // RFC 7636 recommends 43-128 characters
        return CryptographicTokenGenerator.Generate(OAuthSecurityConstants.CodeVerifierByteLength);
    }

    /// <summary>
    /// Generates a SHA256 code challenge from the code verifier.
    /// </summary>
    public static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = Encoding.ASCII.GetBytes(codeVerifier);
        var hash = SHA256.HashData(bytes);
        return CryptographicTokenGenerator.ToUrlSafeBase64(hash);
    }
}
