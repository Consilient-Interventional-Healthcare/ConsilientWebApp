namespace Consilient.Constants
{
    public static class ApplicationConstants
    {
        public static class ConnectionStrings
        {
            public const string Hangfire = "HangfireConnection";
            public const string Default = "DefaultConnection";
        }
        public static class ConfigurationFiles
        {
            public const string AppSettings = "appsettings.json";
            public const string EnvironmentAppSettings = "appsettings.{0}.json";
        }
        public static class ConfigurationSections
        {
            public const string Logging = "Logging";
        }
    }
}
