namespace Consilient.Infrastructure.ExcelImporter
{
    public class ExcelImporterConfiguration
    {
        public required bool CanConvertFile { get; init; } = true;

        public required IEnumerable<string> WorksheetFilters { get; init; } = [];
    }
}