using Microsoft.Extensions.Options;

namespace Consilient.Api.Configuration.Validators
{
    public class AuthenticationOptionsValidator : IValidateOptions<AuthenticationOptions>
    {
        public ValidateOptionsResult Validate(string? name, AuthenticationOptions options)
        {
            var errors = new List<string>();

            if (options.CookieExpiryMinutes <= 0)
                errors.Add($"{AuthenticationOptions.SectionName}:CookieExpiryMinutes must be greater than 0");

            if (options.UserService is null)
                errors.Add($"{AuthenticationOptions.SectionName}:UserService is required");

            if (options.PasswordPolicy is null)
                errors.Add($"{AuthenticationOptions.SectionName}:PasswordPolicy is required");

            return errors.Count > 0
                ? ValidateOptionsResult.Fail(errors)
                : ValidateOptionsResult.Success;
        }
    }
}
