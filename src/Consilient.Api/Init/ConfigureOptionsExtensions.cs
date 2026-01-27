using Consilient.Api.Configuration;
using Consilient.Api.Configuration.Validators;
using Consilient.Users.Services;
using Consilient.Users.Services.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Consilient.Api.Init;

internal static class ConfigureOptionsExtensions
{
    /// <summary>
    /// Registers configuration validators and IOptions bindings with startup validation.
    /// </summary>
    public static IServiceCollection ConfigureApplicationOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register configuration validators
        services.AddSingleton<IValidateOptions<AuthenticationOptions>, AuthenticationOptionsValidator>();
        services.AddSingleton<IValidateOptions<ProviderAssignmentsUploadsOptions>, ProviderAssignmentsUploadsOptionsValidator>();
        services.AddSingleton<IValidateOptions<UserServiceOptions>, UserServiceOptionsValidator>();
        services.AddSingleton<IValidateOptions<TokenGeneratorOptions>, TokenGeneratorOptionsValidator>();

        // Register configuration with IOptions pattern and validation at startup
        services.AddOptions<AuthenticationOptions>()
            .Bind(configuration.GetSection(AuthenticationOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<ProviderAssignmentsUploadsOptions>()
            .Bind(configuration.GetSection(ProviderAssignmentsUploadsOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<UserServiceOptions>()
            .Bind(configuration.GetSection(UserServiceOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<TokenGeneratorOptions>()
            .Bind(configuration.GetSection(TokenGeneratorOptions.SectionName))
            .ValidateOnStart();

        return services;
    }
}
