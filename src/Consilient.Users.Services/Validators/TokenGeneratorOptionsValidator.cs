using Microsoft.Extensions.Options;

namespace Consilient.Users.Services.Validators;

public class TokenGeneratorOptionsValidator : IValidateOptions<TokenGeneratorOptions>
{
    public ValidateOptionsResult Validate(string? name, TokenGeneratorOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Secret))
            errors.Add($"{TokenGeneratorOptions.SectionName}:Secret is required");
        else if (options.Secret.Length < 32)
            errors.Add($"{TokenGeneratorOptions.SectionName}:Secret must be at least 32 characters");

        if (string.IsNullOrWhiteSpace(options.Issuer))
            errors.Add($"{TokenGeneratorOptions.SectionName}:Issuer is required");

        if (string.IsNullOrWhiteSpace(options.Audience))
            errors.Add($"{TokenGeneratorOptions.SectionName}:Audience is required");

        if (options.ExpiryMinutes <= 0)
            errors.Add($"{TokenGeneratorOptions.SectionName}:ExpiryMinutes must be greater than 0");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
