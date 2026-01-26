using Microsoft.Extensions.Options;

namespace Consilient.Users.Services.Validators
{
    public class UserServiceOptionsValidator : IValidateOptions<UserServiceOptions>
    {
        public ValidateOptionsResult Validate(string? name, UserServiceOptions options)
        {
            var errors = new List<string>();

            if (options.Jwt is null)
                errors.Add($"{UserServiceOptions.SectionName}:Jwt configuration is required");

            if (options.AllowedEmailDomains is null || options.AllowedEmailDomains.Length == 0)
                errors.Add($"{UserServiceOptions.SectionName}:AllowedEmailDomains must have at least one entry (use '*' for all)");

            // Conditional: if OAuth enabled, validate OAuth settings
            if (options.OAuth?.Enabled == true)
            {
                if (string.IsNullOrWhiteSpace(options.OAuth.ClientId))
                    errors.Add($"{UserServiceOptions.SectionName}:OAuth:ClientId is required when OAuth is enabled");

                if (string.IsNullOrWhiteSpace(options.OAuth.ClientSecret))
                    errors.Add($"{UserServiceOptions.SectionName}:OAuth:ClientSecret is required when OAuth is enabled");

                if (string.IsNullOrWhiteSpace(options.OAuth.Authority))
                    errors.Add($"{UserServiceOptions.SectionName}:OAuth:Authority is required when OAuth is enabled");
            }

            return errors.Count > 0
                ? ValidateOptionsResult.Fail(errors)
                : ValidateOptionsResult.Success;
        }
    }
}
