namespace Consilient.Constants
{
    public static class ApplicationConstants
    {
        public static class ConfigurationFiles
        {
            public static string AppSettings { get; private set; } = "appsettings.json";
            public static string EnvironmentAppSettings { get; private set; } = "appsettings.{0}.json";
        }

        public static class ConfigurationSections
        {
            public static string Logging { get; private set; } = "Logging";
        }

        public static class ConnectionStrings
        {
            public static string Default { get; private set; } = "DefaultConnection";
            public static string Hangfire { get; private set; } = "HangfireConnection";
        }

        public static class Permissions
        {
            public static string CanApproveVisits { get; private set; } = "CanApproveVisits";
        }

        public static class Roles
        {
            public static string Administrator { get; private set; } = "Administrator";
            public static string NursePracticioner { get; private set; } = "Nurse Practitioner";
            public static string Physician { get; private set; } = "Physician";
            public static string Scribe { get; private set; } = "Scribe";
        }
    }
}
