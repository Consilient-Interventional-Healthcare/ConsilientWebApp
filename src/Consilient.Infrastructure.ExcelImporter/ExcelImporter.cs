using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using static Consilient.Infrastructure.ExcelImporter.ExcelImporter;

namespace Consilient.Infrastructure.ExcelImporter
{
    public class ExcelImporter(ExcelImporterConfiguration configuration, ILogger<ExcelImporter> logger) : ExcelImporterBase<PatientData>(configuration, logger)
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
            public const string CaseId = "Case ID";
            public const string Name = "Name";
            public const string Mrn = "MRN";
            public const string Sex = "Sex";
            public const string Age = "Age";
            public const string Dob = "DOB";
            public const string Room = "Room";
            public const string Bed = "Bed";
            public const string Doa = "DOA";
            public const string Los = "LOS";
            public const string AttendingPhysician = "Attending Physician";
            public const string PrimaryInsurance = "Primary Insurance";
            public const string AdmDx = "AdmDx";

            /// <summary>
            /// An array of the primary expected header names.
            /// </summary>
            public static readonly string[] ExpectedHeaders =
            [
                CaseId, Name, Mrn, Sex, Age, Dob, Room, Bed,
            Doa, Los, AttendingPhysician, PrimaryInsurance, AdmDx
            ];
        }

        protected override (IXLRow? headerRow, Dictionary<string, int>? columnMap) FindHeader(IXLWorksheet worksheet)
        {
            foreach (var row in worksheet.RowsUsed())
            {
                if (!IsHeaderRow(row, out var foundHeaders))
                {
                    continue;
                }
                var columnMap = GetColumnMap(foundHeaders);
                Logger.LogDebug("Header row found on row {RowNumber} in worksheet '{WorksheetName}'.", row.RowNumber(), worksheet.Name);
                return (row, columnMap);
            }
            return (null, null);
        }

        private static bool IsHeaderRow(IXLRow row, out List<string> foundHeaders)
        {
            var headers = row.CellsUsed().Select(c => c.GetValue<string>().Trim()).ToList();
            var hasRequiredHeaders = ExcelHeader.ExpectedHeaders.All(header => headers.Contains(header, StringComparer.OrdinalIgnoreCase));
            foundHeaders = headers;
            return hasRequiredHeaders;
        }

        private static Dictionary<string, int> GetColumnMap(List<string> headers)
        {
            return headers
                .Select((header, index) => new { header, index = index + 1 }) // +1 for 1-based cell index
                .Where(x => !string.IsNullOrWhiteSpace(x.header))
                .ToDictionary(x => x.header, x => x.index, StringComparer.OrdinalIgnoreCase);
        }


        protected override PatientData ExtractEntity(IXLRow row, IDictionary<string, int> columnMap)
        {
            var patient = new PatientData
            {
                CaseId = row.Cell(columnMap[ExcelHeader.CaseId]).GetValue<string>(),
                Name = row.Cell(columnMap[ExcelHeader.Name]).GetValue<string>(),
                Mrn = row.Cell(columnMap[ExcelHeader.Mrn]).GetValue<string>(),
                Sex = row.Cell(columnMap[ExcelHeader.Sex]).GetValue<string>(),
                Age = row.Cell(columnMap[ExcelHeader.Age]).GetValue<int>(),
                Dob = row.Cell(columnMap[ExcelHeader.Dob]).GetValue<DateTime?>(),
                Room = row.Cell(columnMap[ExcelHeader.Room]).GetValue<string>(),
                Bed = row.Cell(columnMap[ExcelHeader.Bed]).GetValue<string>(),
                Doa = row.Cell(columnMap[ExcelHeader.Doa]).GetValue<DateTime>(),
                Los = row.Cell(columnMap[ExcelHeader.Los]).GetValue<int>(),
                AttendingPhysician = row.Cell(columnMap[ExcelHeader.AttendingPhysician]).GetValue<string>(),
                PrimaryInsurance = row.Cell(columnMap[ExcelHeader.PrimaryInsurance]).GetValue<string>(),
                AdmDx = row.Cell(columnMap[ExcelHeader.AdmDx]).GetValue<string>()
            };
            return patient;
        }
    }
}