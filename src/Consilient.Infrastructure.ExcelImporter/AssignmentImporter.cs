using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using System.Globalization;
using static Consilient.Infrastructure.ExcelImporter.AssignmentImporter;

namespace Consilient.Infrastructure.ExcelImporter
{
    public class AssignmentImporter(ExcelImporterConfiguration configuration, ILogger<ExcelImporter> logger) : ExcelImporterBase<AssignmentData>(configuration, logger)
    {
        public class AssignmentData
        {
            public string Name { get; set; } = string.Empty;

            public string Location { get; set; } = string.Empty;

            public string HospitalNumber { get; set; } = string.Empty;

            public DateTime Admit { get; set; }

            public int? LOS { get; set; }

            public string? PsychEval { get; set; }

            public string? AttendingMD { get; set; }

            public string? MedicallyCleared { get; set; }

            public string? NursePractitioner { get; set; }

            public string? Insurance { get; set; }
        }

        static class ExcelHeader
        {
            public static readonly string[] ExpectedHeaders =
            [
                nameof(AssignmentData.Name),
                nameof(AssignmentData.Location),
                nameof(AssignmentData.HospitalNumber),
                nameof(AssignmentData.Admit),
                nameof(AssignmentData.LOS),
                nameof(AssignmentData.PsychEval),
                nameof(AssignmentData.AttendingMD),
                nameof(AssignmentData.MedicallyCleared),
                nameof(AssignmentData.NursePractitioner),
                nameof(AssignmentData.Insurance)
            ];
        }

        protected override (IRow? headerRow, Dictionary<string, int>? columnMap) FindHeader(ISheet worksheet)
        {
            for (int r = worksheet.FirstRowNum; r <= worksheet.LastRowNum; r++)
            {
                var row = worksheet.GetRow(r);
                if (row == null) continue;

                if (!IsHeaderRow(row, out var foundHeaders))
                {
                    continue;
                }

                var columnMap = GetColumnMap(foundHeaders);
                Logger.LogDebug("Header row found on row {RowNumber} in worksheet '{WorksheetName}'.", row.RowNum, worksheet.SheetName);
                return (row, columnMap);
            }

            return (null, null);
        }

        private static bool IsHeaderRow(IRow row, out List<string> foundHeaders)
        {
            var headers = new List<string>();
            var firstCell = row.FirstCellNum >= 0 ? row.FirstCellNum : 0;
            var lastCell = row.LastCellNum >= 0 ? row.LastCellNum - 1 : -1;

            for (int c = firstCell; c <= lastCell; c++)
            {
                var cell = row.GetCell(c);
                var text = GetCellString(cell);
                headers.Add(text);
            }

            var hasRequiredHeaders = ExcelHeader.ExpectedHeaders.All(header => headers.Contains(header, StringComparer.OrdinalIgnoreCase));
            foundHeaders = headers;
            return hasRequiredHeaders;
        }

        private static Dictionary<string, int> GetColumnMap(List<string> headers)
        {
            // NPOI uses 0-based column indices
            return headers
                .Select((header, index) => new { header, index })
                .Where(x => !string.IsNullOrWhiteSpace(x.header))
                .ToDictionary(x => x.header, x => x.index, StringComparer.OrdinalIgnoreCase);
        }

        protected override AssignmentData ExtractEntity(IRow row, IDictionary<string, int> columnMap)
        {
            string GetString(string key) =>
                columnMap.TryGetValue(key, out var idx) ? GetCellString(row.GetCell(idx)) : string.Empty;

            int? GetIntNullable(string key)
            {
                if (!columnMap.TryGetValue(key, out var idx)) return null;
                var cell = row.GetCell(idx);
                return GetCellIntNullable(cell);
            }

            DateTime GetDate(string key)
            {
                if (!columnMap.TryGetValue(key, out var idx)) return default;
                var cell = row.GetCell(idx);
                return GetCellDateTime(cell);
            }

            var patient = new AssignmentData
            {
                Admit = GetDate(nameof(AssignmentData.Admit)),
                AttendingMD = GetString(nameof(AssignmentData.AttendingMD)),
                HospitalNumber = GetString(nameof(AssignmentData.HospitalNumber)),
                Insurance = GetString(nameof(AssignmentData.Insurance)),
                LOS = GetIntNullable(nameof(AssignmentData.LOS)),
                Location = GetString(nameof(AssignmentData.Location)),
                MedicallyCleared = GetString(nameof(AssignmentData.MedicallyCleared)),
                Name = GetString(nameof(AssignmentData.Name)),
                NursePractitioner = GetString(nameof(AssignmentData.NursePractitioner)),
                PsychEval = GetString(nameof(AssignmentData.PsychEval))
            };
            return patient;
        }

        #region Cell Helpers
        private static string GetCellString(ICell? cell)
        {
            if (cell == null) return string.Empty;

            return cell.CellType switch
            {
                CellType.String => cell.StringCellValue?.Trim() ?? string.Empty,
                CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                    ? (cell.DateCellValue?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty)
                    : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                CellType.Boolean => cell.BooleanCellValue ? "TRUE" : "FALSE",
                CellType.Formula => cell.StringCellValue?.Trim() ?? cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                _ => string.Empty
            };
        }

        private static int? GetCellIntNullable(ICell? cell)
        {
            if (cell == null) return null;

            if (cell.CellType == CellType.Numeric)
            {
                return Convert.ToInt32(cell.NumericCellValue);
            }

            var text = GetCellString(cell);
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
            {
                return v;
            }

            return null;
        }

        private static DateTime GetCellDateTime(ICell? cell)
        {
            if (cell == null) return default;
            if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
            {
                return cell.DateCellValue.GetValueOrDefault();
            }

            var text = GetCellString(cell);
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return dt;
            }

            return default;
        }
        #endregion
    }
}
