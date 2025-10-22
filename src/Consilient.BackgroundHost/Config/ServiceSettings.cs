namespace Consilient.BackgroundHost.Config
{
    internal class ServiceSettings
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}
