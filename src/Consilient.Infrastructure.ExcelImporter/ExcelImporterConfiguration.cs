namespace Consilient.Infrastructure.ExcelImporter
{
    public class ExcelImporterConfiguration
    {
        public int MaxRowsToScan { get; init; } = 10_000;
        public string[] Headers { get; init; } = [];
    }
}