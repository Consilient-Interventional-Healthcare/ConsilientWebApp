using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Consilient.Api.Init
{
    internal static partial class ConfigureSwaggerServiceCollectionExtensions
    {
        [GeneratedRegex(@"[^A-Za-z0-9_]", RegexOptions.None)]
        private static partial Regex SchemaIdSanitizer();

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

                c.SupportNonNullableReferenceTypes();

                // Add this to preserve enum names
                c.UseInlineDefinitionsForEnums();

                // Stable, readable schema ids that avoid collisions:
                // - Keep simple DTO/contract type names (for readability)
                // - For other types use a sanitized FullName fallback (namespace + name) with generics expanded
                c.CustomSchemaIds(type =>
                {
                    if (type == null)
                    {
                        return string.Empty;
                    }

                    // Prefer short names for types inside *Contracts* namespaces to keep generated TS clean
                    if (!string.IsNullOrEmpty(type.Namespace) && type.Namespace.Contains(".Contracts"))
                    {
                        return type.Name;
                    }

                    // Handle generics with readable suffix (e.g. ResultOfFoo_Bar)
                    if (type.IsGenericType)
                    {
                        var genericBase = type.Name.Split('`')[0];
                        var genericArgs = string.Join("_", type.GetGenericArguments().Select(t => (t.IsGenericType ? t.Name.Split('`')[0] : t.Name)));
                        var candidate = $"{genericBase}_{genericArgs}";
                        // sanitize and return
                        return SchemaIdSanitizer().Replace(candidate, "_");
                    }

                    // Fallback: use full name (namespace + name) and sanitize
                    var full = type.FullName ?? $"{type.Namespace}.{type.Name}";
                    var safe = full.Replace("+", "_"); // nested types
                    safe = SchemaIdSanitizer().Replace(safe, "_");
                    return safe;
                });

                // Mark non-nullable properties (including value types) as required
                c.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
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

    /// <summary>
    /// Schema filter to mark non-nullable properties as required in OpenAPI schema.
    /// </summary>
    internal class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
    {
        public void Apply(Microsoft.OpenApi.Models.OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null || schema.Properties.Count == 0)
            {
                return;
            }

            var nullabilityContext = new NullabilityInfoContext();

            foreach (var property in context.Type.GetProperties())
            {
                var nullabilityInfo = nullabilityContext.Create(property);
                var schemaPropertyName = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];

                if (schema.Properties.ContainsKey(schemaPropertyName))
                {
                    // Mark as required if the property is non-nullable
                    if (nullabilityInfo.WriteState == System.Reflection.NullabilityState.NotNull)
                    {
                        schema.Required ??= new HashSet<string>();
                        schema.Required.Add(schemaPropertyName);
                    }
                }
            }
        }
    }
}