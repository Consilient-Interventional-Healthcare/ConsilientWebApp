using System.Security.Cryptography;

namespace Consilient.Users.Services.OAuth
{
    /// <summary>
    /// Utility for generating cryptographically secure tokens.
    /// </summary>
    public static class CryptographicTokenGenerator
    {
        /// <summary>
        /// Generates a URL-safe base64 token of specified byte length.
        /// </summary>
        /// <param name="sizeInBytes">The size of the random byte array (default: 32 bytes = 256 bits).</param>
        /// <returns>A URL-safe base64 encoded token.</returns>
        public static string Generate(int sizeInBytes = 32)
        {
            var bytes = RandomNumberGenerator.GetBytes(sizeInBytes);
            return ToUrlSafeBase64(bytes);
        }

        /// <summary>
        /// Converts a byte array to URL-safe base64 encoding.
        /// Replaces +/= characters per RFC 4648 Section 5.
        /// </summary>
        /// <param name="bytes">The byte array to encode.</param>
        /// <returns>URL-safe base64 encoded string.</returns>
        public static string ToUrlSafeBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}