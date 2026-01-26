namespace Consilient.Api.Configuration
{
    /// <summary>
    /// Configuration options for allowed origins (CORS).
    /// </summary>
    public class AllowedOriginsOptions
    {
        public const string SectionName = "AllowedOrigins";

        /// <summary>
        /// List of allowed origins for CORS requests.
        /// </summary>
        public string[] Origins { get; init; } = [];
    }
}
