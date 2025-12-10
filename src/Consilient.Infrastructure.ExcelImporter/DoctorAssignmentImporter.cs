using Consilient.Infrastructure.ExcelImporter.NPOI;
using Microsoft.Extensions.Logging;
using static Consilient.Infrastructure.ExcelImporter.DoctorAssignmentImporter;

namespace Consilient.Infrastructure.ExcelImporter
{
    public class DoctorAssignmentImporter(ILogger<DoctorAssignmentImporter> logger) : ExcelImporterBase<PatientData>(new ExcelSheetReader(), new ExcelImporterConfiguration(), logger)
    {
        /// <summary>
        /// Represents the data structure for a patient record imported from an Excel file.
        /// </summary>
        public class PatientData
        {
            /// <summary>
            /// Gets or sets the Case ID.
            /// </summary>
            public string CaseId { get; init; } = string.Empty;
            /// <summary>
            /// Gets or sets the patient's name.
            /// </summary>
            public string Name { get; init; } = string.Empty;
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            /// <summary>
            /// Gets or sets the Medical Record Number.
            /// </summary>
            public string Mrn { get; init; } = string.Empty;
            /// <summary>
            /// Gets or sets the patient's sex.
            /// </summary>
            public string Sex { get; init; } = string.Empty;
            /// <summary>
            /// Gets or sets the patient's age.
            /// </summary>
            public int Age { get; init; }
            /// <summary>
            /// Gets or sets the patient's Date of Birth. This can be nullable.
            /// </summary>
            public DateTime? Dob { get; init; }
            /// <summary>
            /// Gets or sets the patient's room number.
            /// </summary>
            public string Room { get; init; } = string.Empty;
            /// <summary>
            /// Gets or sets the patient's bed identifier.
            /// </summary>
            public string Bed { get; init; } = string.Empty;
            /// <summary>
            /// Gets or sets the Date of Admission.
            /// </summary>
            public DateTime Doa { get; init; }
            /// <summary>
            /// Gets or sets the Length of Stay in days.
            /// </summary>
            public int Los { get; init; }
            /// <summary>
            /// Gets or sets the attending physician's name.
            /// </summary>
            public string AttendingPhysician { get; init; } = string.Empty;
            /// <summary>
            /// Gets or sets the primary insurance provider.
            /// </summary>
            public string PrimaryInsurance { get; init; } = string.Empty;
            /// <summary>
            /// Gets or sets the admitting diagnosis.
            /// </summary>
            public string AdmDx { get; init; } = string.Empty;
            public DateOnly? ServiceDate { get; init; }
            public int? FacilityId { get; init; }
        }

        static class ExcelHeader
        {
            // Headers provided by user
            public const string NameDown = "Name↓";
            public const string Location = "Location";
            public const string HospitalNumber = "Hospital Number";
            public const string Admit = "Admit";
            public const string Los = "LOS";
            public const string Mrn = "MRN";
            public const string Age = "Age";
            public const string Dob = "DOB";
            public const string HandP = "H&P";
            public const string PsychEval = "Psych Eval";
            public const string AttendingMd = "Attending MD";
            public const string Cleared = "Cleared";
            public const string NursePractitioner = "Nurse Practitioner";
            public const string Insurance = "Insurance";

            /// <summary>
            /// An array of the primary expected header names.
            /// </summary>
            public static readonly string[] ExpectedHeaders =
            [
                NameDown, Location, HospitalNumber, Admit, Los, Mrn, Age, Dob,
                HandP, PsychEval, AttendingMd, Cleared, NursePractitioner, Insurance
            ];
        }

        //protected override (IRow? headerRow, Dictionary<string, int>? columnMap) FindHeader(ISheet worksheet)
        //{
        //    for (int r = worksheet.FirstRowNum; r <= worksheet.LastRowNum; r++)
        //    {
        //        var row = worksheet.GetRow(r);
        //        if (row == null) continue;

        //        if (!IsHeaderRow(row, out var foundHeaders))
        //        {
        //            continue;
        //        }

        //        var columnMap = GetColumnMap(foundHeaders);
        //        Logger.LogDebug("Header row found on row {RowNumber} in worksheet '{WorksheetName}'.", row.RowNum, worksheet.SheetName);
        //        return (row, columnMap);
        //    }

        //    return (null, null);
        //}

        //private static bool IsHeaderRow(IRow row, out List<string> foundHeaders)
        //{
        //    var headers = new List<string>();
        //    var firstCell = row.FirstCellNum >= 0 ? row.FirstCellNum : 0;
        //    var lastCell = row.LastCellNum >= 0 ? row.LastCellNum - 1 : -1;

        //    for (int c = firstCell; c <= lastCell; c++)
        //    {
        //        var cell = row.GetCell(c);
        //        var text = GetCellString(cell);
        //        headers.Add(text);
        //    }

        //    var hasRequiredHeaders = ExcelHeader.ExpectedHeaders.All(header => headers.Contains(header, StringComparer.OrdinalIgnoreCase));
        //    foundHeaders = headers;
        //    return hasRequiredHeaders;
        //}

        //private static Dictionary<string, int> GetColumnMap(List<string> headers)
        //{
        //    // NPOI uses 0-based column indices
        //    return headers
        //        .Select((header, index) => new { header, index })
        //        .Where(x => !string.IsNullOrWhiteSpace(x.header))
        //        .ToDictionary(x => x.header, x => x.index, StringComparer.OrdinalIgnoreCase);
        //}

        //protected override PatientData ExtractEntity(IRow row, IDictionary<string, int> columnMap)
        //{
        //    string GetString(string key) =>
        //        columnMap.TryGetValue(key, out var idx) ? GetCellString(row.GetCell(idx)) : string.Empty;

        //    int GetInt(string key)
        //    {
        //        if (!columnMap.TryGetValue(key, out var idx)) return 0;
        //        var cell = row.GetCell(idx);
        //        return GetCellInt(cell);
        //    }

        //    DateTime? GetNullableDate(string key)
        //    {
        //        if (!columnMap.TryGetValue(key, out var idx)) return null;
        //        var cell = row.GetCell(idx);
        //        return GetCellDateTimeNullable(cell);
        //    }

        //    DateTime GetDate(string key)
        //    {
        //        if (!columnMap.TryGetValue(key, out var idx)) return default;
        //        var cell = row.GetCell(idx);
        //        return GetCellDateTime(cell);
        //    }

        //    var patient = new PatientData
        //    {
        //        // Map the incoming headers to patient properties:
        //        CaseId = GetString(ExcelHeader.HospitalNumber),
        //        Name = GetString(ExcelHeader.NameDown),
        //        Mrn = GetString(ExcelHeader.Mrn),
        //        Sex = string.Empty, // no explicit Sex column in provided headers
        //        Age = GetInt(ExcelHeader.Age),
        //        Dob = GetNullableDate(ExcelHeader.Dob),
        //        Room = GetString(ExcelHeader.Location),
        //        Bed = string.Empty,
        //        Doa = GetDate(ExcelHeader.Admit),
        //        Los = GetInt(ExcelHeader.Los),
        //        AttendingPhysician = GetString(ExcelHeader.AttendingMd),
        //        PrimaryInsurance = GetString(ExcelHeader.Insurance),
        //        AdmDx = GetString(ExcelHeader.HandP)
        //    };
        //    return patient;
        //}

        //#region Cell Helpers
        //private static string GetCellString(ICell? cell, IFormulaEvaluator? evaluator = null)
        //{
        //    if (cell == null) return string.Empty;

        //    // If formula and evaluator provided, evaluate to get a value cell
        //    if (cell.CellType == CellType.Formula && evaluator != null)
        //    {
        //        var evaluated = evaluator.Evaluate(cell);
        //        if (evaluated != null)
        //        {
        //            switch (evaluated.CellType)
        //            {
        //                case CellType.String:
        //                    return evaluated.StringValue?.Trim() ?? string.Empty;
        //                case CellType.Numeric:
        //                    // If the original cell is formatted as date, try to convert
        //                    if (DateUtil.IsCellDateFormatted(cell))
        //                    {
        //                        if (cell.DateCellValue.HasValue)
        //                            return cell.DateCellValue.Value.ToString("O", CultureInfo.InvariantCulture);
        //                    }
        //                    return evaluated.NumberValue.ToString(CultureInfo.InvariantCulture);
        //                case CellType.Boolean:
        //                    return evaluated.BooleanValue ? "TRUE" : "FALSE";
        //                case CellType.Error:
        //                    return string.Empty;
        //            }
        //        }
        //    }

        //    // No evaluator or not using evaluator: use cached result type safely
        //    return cell.CellType switch
        //    {
        //        CellType.String => cell.StringCellValue?.Trim() ?? string.Empty,
        //        CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
        //            ? cell.DateCellValue?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty
        //            : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
        //        CellType.Boolean => cell.BooleanCellValue ? "TRUE" : "FALSE",
        //        CellType.Formula => cell.CachedFormulaResultType switch
        //        {
        //            CellType.String => cell.StringCellValue?.Trim() ?? string.Empty,
        //            CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
        //                ? cell.DateCellValue?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty
        //                : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
        //            CellType.Boolean => cell.BooleanCellValue ? "TRUE" : "FALSE",
        //            _ => string.Empty
        //        },
        //        _ => string.Empty
        //    };
        //}

        //private static int GetCellInt(ICell? cell)
        //{
        //    if (cell == null) return 0;

        //    if (cell.CellType == CellType.Numeric)
        //    {
        //        return Convert.ToInt32(cell.NumericCellValue);
        //    }

        //    var text = GetCellString(cell);
        //    if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
        //    {
        //        return v;
        //    }

        //    return 0;
        //}

        //private static DateTime GetCellDateTime(ICell? cell)
        //{
        //    if (cell == null) return default;
        //    if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
        //    {
        //        // Fix: Use .GetValueOrDefault() to handle nullable DateTime
        //        return cell.DateCellValue.GetValueOrDefault();
        //    }

        //    var text = GetCellString(cell);
        //    if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        //    {
        //        return dt;
        //    }

        //    return default;
        //}

        //private static DateTime? GetCellDateTimeNullable(ICell? cell)
        //{
        //    if (cell == null) return null;
        //    if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
        //    {
        //        return cell.DateCellValue;
        //    }

        //    var text = GetCellString(cell);
        //    if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        //    {
        //        return dt;
        //    }

        //    return null;
        //}
        //#endregion
    }
}