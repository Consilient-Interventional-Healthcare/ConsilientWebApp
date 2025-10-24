namespace Consilient.Infrastructure.ExcelImporter
{
    public class ExcelImporterConfiguration
    {
        public bool CanConvertFile { get; set; } = true;

        public IEnumerable<string> WorksheetFilters { get; set; } = [];
    }
}