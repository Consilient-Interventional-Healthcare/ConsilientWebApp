namespace Consilient.ProviderAssignments.Services.Import.Validation
{
    /// <summary>
    /// Provides validator instances for Excel row validation during import.
    /// </summary>
    public interface IValidatorProvider
    {
        /// <summary>
        /// Gets all registered validators for raw Excel row validation.
        /// </summary>
        /// <returns>An enumerable of validators.</returns>
        IEnumerable<IExcelRowValidator> GetValidators();
    }
}
