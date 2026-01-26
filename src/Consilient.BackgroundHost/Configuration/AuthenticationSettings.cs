namespace Consilient.BackgroundHost.Configuration
{
    internal class AuthenticationSettings
    {
        public const string SectionName = "Authentication";

        /// <summary>
        /// Master switch for dashboard authentication. Default true.
        /// </summary>
        public bool DashboardAuthEnabled { get; init; } = true;

        /// <summary>
        /// When true, forces Entra authentication even when not running in Azure.
        /// Use this to test Entra auth flow locally.
        /// </summary>
        public bool ForceEntraAuth { get; init; } = false;

        public UserServiceSettings? UserService { get; init; }
    }
}
