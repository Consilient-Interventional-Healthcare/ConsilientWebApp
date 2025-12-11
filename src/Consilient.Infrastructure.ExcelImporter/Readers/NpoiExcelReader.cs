using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Models;
using NPOI.SS.UserModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Consilient.Infrastructure.ExcelImporter.Readers
{

    public class NpoiExcelReader : IExcelReader
    {
        public async IAsyncEnumerable<ExcelRow> ReadRowsAsync(
            Stream stream,
            SheetSelector sheetSelector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // NPOI is synchronous, but wrap for async enumerable
            await Task.Yield(); // Allow async unwinding

            using var workbook = WorkbookFactory.Create(stream);
            var sheet = GetSheet(workbook, sheetSelector);

            if (sheet == null)
            {
                throw new InvalidOperationException($"Sheet not found: {sheetSelector}");
            }

            var headerRow = FindHeaderRow(sheet);
            if (headerRow == null)
            {
                throw new InvalidOperationException("Header row not found");
            }

            var headers = ExtractHeaders(headerRow);

            for (var rowNum = headerRow.RowNum + 1; rowNum <= sheet.LastRowNum; rowNum++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var row = sheet.GetRow(rowNum);
                if (row == null)
                {
                    continue;
                }

                var cells = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                for (var i = 0; i < headers.Count; i++)
                {
                    var header = headers[i];
                    if (string.IsNullOrWhiteSpace(header))
                    {
                        continue;
                    }

                    var cell = row.GetCell(i);
                    cells[header] = GetCellValue(cell);
                }

                // Skip completely empty rows
                if (cells.Values.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                yield return new ExcelRow(rowNum, cells);
            }
        }

        private static ISheet? GetSheet(IWorkbook workbook, SheetSelector selector)
        {
            return selector switch
            {
                { Index: not null } => workbook.GetSheetAt(selector.Index.Value),
                { Name: not null } => workbook.GetSheet(selector.Name),
                _ => workbook.GetSheetAt(0)
            };
        }

        private static IRow? FindHeaderRow(ISheet sheet)
        {
            // Simple: assume first row is header
            return sheet.GetRow(sheet.FirstRowNum);
        }

        private static List<string> ExtractHeaders(IRow row)
        {
            var headers = new List<string>();
            for (var i = 0; i < row.LastCellNum; i++)
            {
                var cell = row.GetCell(i);
                headers.Add(GetCellValue(cell));
            }
            return headers;
        }

        private static string GetCellValue(ICell? cell)
        {
            if (cell == null)
            {
                return string.Empty;
            }

            return cell.CellType switch
            {
                CellType.String => cell.StringCellValue.Trim(),
                CellType.Numeric when DateUtil.IsCellDateFormatted(cell) =>
                    string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd HH:mm:ss}", cell.DateCellValue),
                CellType.Numeric => cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                CellType.Formula => GetFormulaValue(cell),
                _ => string.Empty
            };
        }

        private static string GetFormulaValue(ICell cell)
        {
            try
            {
                return cell.CachedFormulaResultType switch
                {
                    CellType.String => cell.StringCellValue,
                    CellType.Numeric => cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                    CellType.Boolean => cell.BooleanCellValue.ToString(),
                    _ => string.Empty
                };
            }
            catch
            {
                return string.Empty;
            }
        }
    }

}