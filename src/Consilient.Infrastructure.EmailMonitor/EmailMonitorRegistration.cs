using Consilient.Infrastructure.EmailMonitor.Contracts;
using Consilient.Infrastructure.EmailMonitor.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Infrastructure.EmailMonitor
{
    public static class EmailMonitorRegistration
    {
        public static void RegisterEmailMonitorServices(this IServiceCollection services, MonitorConfiguration monitorConfiguration)
        {
            services.AddScoped<IEmailProcessor, ExtractAttachmentsEmailProcessor>(sp => ActivatorUtilities.CreateInstance<ExtractAttachmentsEmailProcessor>(sp, monitorConfiguration.ExtractAttachmentsEmailProcessor));
            services.AddScoped<IEmailMonitor>(sp => ActivatorUtilities.CreateInstance<EmailMonitor>(sp, monitorConfiguration));
        }
    }
}
