using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Infrastructure.Injection
{
    public static class ApplicationSettingsRegistrationExtensions
    {
        public static TApplicationSettings RegisterApplicationSettings<TApplicationSettings>(this IServiceCollection services, IConfiguration configuration, string? key = null) where TApplicationSettings : class
        {
            var keyStr = key ?? typeof(TApplicationSettings).Name;
            var applicationSettings = Activator.CreateInstance<TApplicationSettings>();
            configuration.GetSection(keyStr).Bind(applicationSettings);
            services.AddSingleton(applicationSettings);
            return applicationSettings;
        }
    }
}
