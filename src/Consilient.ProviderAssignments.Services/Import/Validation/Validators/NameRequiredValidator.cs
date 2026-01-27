using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Validators;
using Consilient.ProviderAssignments.Contracts.Import;
using System.Text.RegularExpressions;

namespace Consilient.ProviderAssignments.Services.Import.Validation.Validators;

public partial class NameRequiredValidator : RowValidator<ExcelProviderAssignmentRow>, IExcelRowValidator
{
    public override ValidationResult Validate(ExcelProviderAssignmentRow row, int rowNumber)
    {
        if (!IsValidName(row.Name))
            return ValidationResult.Failed(Error(rowNumber, nameof(row.Name), "Name is required"));

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a name contains at least one word with at least two letters.
    /// </summary>
    internal static bool IsValidName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && ValidNameRegex().IsMatch(name);

    // Matches at least 2 consecutive letters
    [GeneratedRegex(@"[a-zA-Z]{2,}", RegexOptions.Compiled)]
    private static partial Regex ValidNameRegex();
}
