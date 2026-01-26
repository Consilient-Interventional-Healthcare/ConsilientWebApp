namespace Consilient.Infrastructure.ExcelImporter.Contracts
{

    public record ImportOptions
    {
        public required SheetSelector Sheet { get; init; }
        public required ColumnMapping ColumnMapping { get; init; }
        public int BatchSize { get; init; } = 1000;
        public int MaxRows { get; init; } = int.MaxValue;
        public bool SkipEmptyRows { get; init; } = true;
        public bool FailOnValidationError { get; init; } = true;

        /// <summary>
        /// Optional function to determine when to stop reading rows.
        /// Takes the current ExcelRow and ColumnMapping, returns true to stop reading.
        /// Checked before mapping and validation.
        /// </summary>
        public Func<ExcelRow, ColumnMapping, bool>? ShouldStopReading { get; init; }
    }

}
