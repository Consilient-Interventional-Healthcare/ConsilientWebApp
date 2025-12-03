using Microsoft.OpenApi.Models;

namespace Consilient.Api.Init
{
    internal static class ConfigureSwaggerServiceCollectionExtensions
    {
        public static void AddSwaggerGen(this IServiceCollection services, string appId, string apiVersion)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(apiVersion, new OpenApiInfo
                {
                    Title = appId,
                    Version = apiVersion
                });
            });
        }

        public static void UseSwagger(this IApplicationBuilder app, string appId, string apiVersion)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"{appId} {apiVersion}");
                c.DocumentTitle = appId;
                c.RoutePrefix = string.Empty;
            });
        }
    }
}
