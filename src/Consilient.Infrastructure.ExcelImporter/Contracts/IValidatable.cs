namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    /// <summary>
    /// Interface for rows that can carry validation errors through the import pipeline.
    /// </summary>
    /// <remarks>
    /// Deprecated. Use <see cref="ValidatedRow{T}"/> wrapper instead to separate validation state from data model.
    /// </remarks>
    [Obsolete("Use ValidatedRow<T> wrapper to carry validation state. This interface will be removed in a future release.")]
    public interface IValidatable
    {
        /// <summary>
        /// Gets or sets the list of validation error messages for this row.
        /// </summary>
        List<string> ValidationErrors { get; set; }
    }
}
