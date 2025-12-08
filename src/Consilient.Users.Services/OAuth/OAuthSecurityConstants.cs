namespace Consilient.Users.Services.OAuth
{
    /// <summary>
    /// Security constants for OAuth and PKCE flows.
    /// </summary>
    public static class OAuthSecurityConstants
    {
        /// <summary>
        /// Standard token size in bytes (32 bytes = 256 bits).
        /// Results in ~43 URL-safe base64 characters, compliant with RFC 7636.
        /// </summary>
        public const int TokenSizeBytes = 32;

        /// <summary>
        /// CSRF token cookie lifetime in minutes.
        /// </summary>
        public const int CsrfTokenExpirationMinutes = 15;

        /// <summary>
        /// OAuth state token lifetime in minutes.
        /// Should be long enough for user to complete OAuth flow but short enough to limit exposure.
        /// </summary>
        public const int StateExpirationMinutes = 10;

        /// <summary>
        /// PKCE code verifier size in bytes.
        /// Must meet RFC 7636 requirements (43-128 characters after encoding).
        /// </summary>
        public const int CodeVerifierByteLength = 32;

        /// <summary>
        /// Cleanup interval for expired state entries in minutes (in-memory store only).
        /// </summary>
        public const int CleanupIntervalMinutes = 5;
    }
}