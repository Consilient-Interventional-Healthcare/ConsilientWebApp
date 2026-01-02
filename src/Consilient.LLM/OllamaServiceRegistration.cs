using Consilient.LLM.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.LLM
{
    public static class OllamaServiceRegistration
    {
        public static void RegisterOllamaServices(this IServiceCollection services, OllamaServiceConfiguration ollamaServiceConfiguration)
        {
            services.AddScoped<IOllamaService>(_ => new OllamaService(ollamaServiceConfiguration));
        }
    }
}
