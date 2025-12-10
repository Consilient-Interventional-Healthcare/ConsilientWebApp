using Consilient.Infrastructure.ExcelImporter.Contracts;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using System.Text.RegularExpressions;

namespace Consilient.Infrastructure.ExcelImporter
{
    public abstract class ExcelImporterBase<TData>(
        ExcelImporterConfiguration configuration,
        ILogger logger) : IExcelImporter<TData>
        where TData : class, new()
    {
        protected ILogger Logger { get; } = logger;

        public IEnumerable<TData> Import(string filename)
        {
            Logger.LogInformation("Starting Excel import for file: {FileName}", filename);

            using var workbook = Helpers.WorkbookFactory.Create(configuration.CanConvertFile, filename);

            //var filteredWorksheets =
            //    FilterWorksheets(GetWorksheets(workbook), [.. configuration.WorksheetFilters]).ToList();
            //Logger.LogInformation("Found {WorksheetCount} worksheets to process after filtering.",
            //    filteredWorksheets.Count);

            //var allPatients = filteredWorksheets.SelectMany(ProcessWorksheet).ToList();
            var sheet = workbook.GetSheetAt(0);
            var allPatientsList = ProcessWorksheet(sheet);

            Logger.LogInformation("Excel import finished for file: {FileName}", filename);
            return allPatientsList;
        }

        //private static IEnumerable<ISheet> GetWorksheets(IWorkbook workbook)
        //{
        //    for (int i = 0; i < workbook.NumberOfSheets; i++)
        //    {
        //        yield return workbook.GetSheetAt(i);
        //    }
        //}

        //internal static IEnumerable<ISheet> FilterWorksheets(IEnumerable<ISheet> worksheets,
        //    IList<string> worksheetFilters)
        //{
        //    if (!worksheetFilters.Any())
        //    {
        //        return worksheets;
        //    }

        //    var patterns = worksheetFilters.Select(filter => new Regex(filter, RegexOptions.IgnoreCase)).ToList();

        //    return worksheets.Where(ws => patterns.Any(p => p.IsMatch(ws.SheetName)));
        //}

        protected abstract TData ExtractEntity(IRow row, IDictionary<string, int> columnMap);

        protected abstract (IRow? headerRow, Dictionary<string, int>? columnMap) FindHeader(ISheet worksheet);

        private List<TData> ProcessDataRows(ISheet worksheet, IRow headerRow, Dictionary<string, int> columnMap)
        {
            var rows = new List<TData>();
            var firstRow = headerRow.RowNum + 1;
            var lastRow = worksheet.LastRowNum;
            for (int r = firstRow; r <= lastRow; r++)
            {
                var row = worksheet.GetRow(r);
                if (row == null) continue;

                try
                {
                    var entity = ExtractEntity(row, columnMap);
                    rows.Add(entity);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error processing row {RowNumber} in worksheet '{WorksheetName}'.",
                        row.RowNum, worksheet.SheetName);
                }
            }

            return rows;
        }

        private List<TData> ProcessWorksheet(ISheet worksheet)
        {
            Logger.LogDebug("Processing worksheet: {WorksheetName}", worksheet.SheetName);

            var (headerRow, columnMap) = FindHeader(worksheet);
            if (headerRow != null && columnMap != null)
            {
                return ProcessDataRows(worksheet, headerRow, columnMap);
            }
            Logger.LogWarning("Header row not found in worksheet '{WorksheetName}'. Skipping.", worksheet.SheetName);
            return [];
        }

        protected IEnumerable<string> GetHeaders()
        {
            return typeof(TData)
                .GetProperties()
                .Select(p => p.Name);
        }
    }
}