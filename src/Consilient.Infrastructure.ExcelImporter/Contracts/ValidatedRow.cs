namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    /// <summary>
    /// Generic wrapper that associates a row with its validation state.
    /// Separates validation concerns from the data model.
    /// </summary>
    public record ValidatedRow<T> where T : class
    {
        public required T Row { get; init; }
        public required int RowNumber { get; init; }
        public List<string> Errors { get; init; } = [];
        public bool IsValid => Errors.Count == 0;

        public ValidatedRow<T> WithErrors(IEnumerable<string> errors) =>
            this with { Errors = [.. Errors, .. errors] };
    }
}
