using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Consilient.Infrastructure.Serialization;

/// <summary>
/// Provides shared JSON serialization configuration for the application.
/// Ensures consistent camelCase property naming across all services using Newtonsoft.Json.
/// </summary>
public static class JsonSerializerConfiguration
{
    /// <summary>
    /// Default JSON serializer settings with camelCase naming strategy.
    /// Use this for all JsonConvert.SerializeObject/DeserializeObject calls.
    /// </summary>
    public static readonly JsonSerializerSettings DefaultSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None
    };

    /// <summary>
    /// Configures the provided JsonSerializerSettings with the default settings.
    /// Use this to apply consistent settings to ASP.NET Core JSON options.
    /// </summary>
    /// <param name="settings">The JsonSerializerSettings to configure</param>
    public static void Configure(JsonSerializerSettings settings)
    {
        settings.ContractResolver = DefaultSettings.ContractResolver;
        settings.NullValueHandling = DefaultSettings.NullValueHandling;
        settings.Formatting = DefaultSettings.Formatting;
    }

    /// <summary>
    /// Initializes the global Newtonsoft.Json default settings.
    /// Call this once at application startup before any JSON operations.
    /// </summary>
    public static void InitializeGlobalDefaults()
    {
        JsonConvert.DefaultSettings = () => DefaultSettings;
    }
}
