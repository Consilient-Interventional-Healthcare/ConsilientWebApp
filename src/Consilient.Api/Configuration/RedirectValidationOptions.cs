namespace Consilient.Api.Configuration
{
    /// <summary>
    /// Configuration for validating redirect URLs to prevent open redirect attacks.
    /// </summary>
    public class RedirectValidationOptions
    {
        /// <summary>
        /// List of allowed origins for redirect URLs. Should match CORS allowed origins.
        /// </summary>
        public string[] AllowedOrigins { get; set; } = [];
    }
}