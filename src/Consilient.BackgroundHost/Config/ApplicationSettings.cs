namespace Consilient.BackgroundHost.Config
{
    internal class ApplicationSettings
    {
        public ServiceSettings Service { get; set; } = new();
        //public bool IsProduction => Environment.Equals("production", StringComparison.CurrentCultureIgnoreCase);
    }
}
