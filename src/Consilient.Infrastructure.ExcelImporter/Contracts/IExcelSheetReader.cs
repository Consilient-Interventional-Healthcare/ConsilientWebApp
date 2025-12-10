namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    public interface IExcelSheetReader
    {
        SheetReadResult ReadSheet(string fileName, ExcelImporterConfiguration configuration);
    }
}