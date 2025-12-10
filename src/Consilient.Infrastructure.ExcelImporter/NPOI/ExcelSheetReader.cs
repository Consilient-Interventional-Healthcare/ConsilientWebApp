using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Consilient.Infrastructure.ExcelImporter.Contracts;
using NPOI.SS.UserModel;

namespace Consilient.Infrastructure.ExcelImporter.NPOI
{
    internal sealed class ExcelSheetReader : IExcelSheetReader
    {
        private readonly CellValueExtractor _cellExtractor = new();

        public SheetReadResult ReadSheet(string fileName, ExcelImporterConfiguration configuration)
        {
            using var file = OpenFile(fileName);
            var sheet = GetSheet(file);
            var maxScan = Math.Max(1, configuration.MaxRowsToScan);
            var firstRow = Math.Max(0, sheet.FirstRowNum);
            var lastRow = sheet.LastRowNum;
            var dataTable = new DataTable();
            Dictionary<string, int>? columnMap = null;

            for (var r = firstRow; r <= lastRow && r - firstRow < maxScan; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;

                var headers = ExtractRowHeaders(row);
                if (IsHeaderRow(headers, configuration.Headers))
                {
                    columnMap = CreateColumnMap(headers);

                    // create DataTable columns in column order
                    foreach (var kv in columnMap.OrderBy(kv => kv.Value))
                    {
                        // use header name as column name
                        dataTable.Columns.Add(kv.Key, typeof(string));
                    }

                    // populate data rows following header
                    var start = row.RowNum + 1;
                    var lastToRead = Math.Min(lastRow, start + Math.Max(0, configuration.MaxRowsToScan));
                    for (var rr = start; rr <= lastToRead; rr++)
                    {
                        var dataRow = sheet.GetRow(rr);
                        if (dataRow == null) continue;

                        var values = new object[columnMap.Count];
                        var ordered = columnMap.OrderBy(kv => kv.Value).ToArray();
                        for (var i = 0; i < ordered.Length; i++)
                        {
                            var colIndex = ordered[i].Value;
                            var cell = dataRow.GetCell(colIndex);
                            var s = _cellExtractor.GetString(cell);
                            values[i] = string.IsNullOrWhiteSpace(s) ? DBNull.Value : (object)s;
                        }

                        dataTable.Rows.Add(values);
                    }

                    // header found and processed — break scanning loop
                    break;
                }
            }

            return new SheetReadResult
            {
                ColumnMap = columnMap,
                DataTable = dataTable
            };
        }

        private static ISheet GetSheet(IWorkbook file)
        {
            return file.GetSheetAt(0);
        }

        private static bool IsHeaderRow(List<string> foundHeaders, string[] expectedHeaders) =>
            expectedHeaders != null && expectedHeaders.Length > 0 && expectedHeaders.All(h => foundHeaders.Contains(h, StringComparer.OrdinalIgnoreCase));

        private static IWorkbook OpenFile(string fileName)
        {
            return WorkbookFactory.Create(fileName);
        }

        private List<string> ExtractRowHeaders(IRow row)
        {
            var headers = new List<string>();
            var firstCell = row.FirstCellNum >= 0 ? row.FirstCellNum : 0;
            var lastCell = row.LastCellNum >= 0 ? row.LastCellNum - 1 : -1;

            for (var c = firstCell; c <= lastCell; c++)
            {
                var cell = row.GetCell(c);
                headers.Add(_cellExtractor.GetString(cell));
            }

            return headers;
        }

        private static Dictionary<string, int> CreateColumnMap(List<string> headers) =>
            headers
                .Select((header, index) => new { header = header?.Trim(), index })
                .Where(x => !string.IsNullOrWhiteSpace(x.header))
                .ToDictionary(x => x.header!, x => x.index, StringComparer.OrdinalIgnoreCase);
    }
}