using Consilient.Background.Workers.Contracts;
using Consilient.Background.Workers.Models;
using Consilient.Infrastructure.ExcelImporter;
using Consilient.Infrastructure.ExcelImporter.Helpers;
using Consilient.Infrastructure.ExcelImporter.Models;
using Hangfire.Server;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace Consilient.Background.Workers
{
    public class ImportClinicalDataSheetWorker(ExcelImporter excelImporter, string connectionString) : IBackgroundWorker
    {
        private readonly string _connectionString = connectionString;
        private readonly ExcelImporter _excelImporter = excelImporter;

        // Stage constants
        private const string _stageInitializing = "Initializing";
        private const string _stageReadingFile = "ReadingFile";
        private const string _stageProcessingRecords = "ProcessingRecords";
        private const string _stageSavingData = "SavingData";
        private const string _stageCompleted = "Completed";
        private const string _stageFailed = "Failed";

        // Event for progress reporting using the reusable WorkerProgressEventArgs
        public event EventHandler<WorkerProgressEventArgs>? ProgressChanged;

        public async Task Import(string filePath, DateOnly serviceDate, int facilityId, PerformContext context)
        {
            var jobId = context.BackgroundJob.Id;
            try
            {

                // Report initialization
                OnProgressChanged(new WorkerProgressEventArgs
                {
                    JobId = jobId,
                    Stage = _stageInitializing,
                    CurrentOperation = "Starting import process"
                });

                // Read Excel file
                OnProgressChanged(new WorkerProgressEventArgs
                {
                    JobId = jobId,
                    Stage = _stageReadingFile,
                    CurrentOperation = $"Reading file: {Path.GetFileName(filePath)}"
                });

                var records = _excelImporter.Import(filePath);
                var recordList = records.ToList();
                var totalRecords = recordList.Count;

                // Process records
                OnProgressChanged(new WorkerProgressEventArgs
                {
                    JobId = jobId,
                    Stage = _stageProcessingRecords,
                    TotalItems = totalRecords,
                    ProcessedItems = 0,
                    CurrentOperation = "Processing patient records"
                });

                FillFields(recordList, (r) =>
                {
                    var (FirstName, LastName) = ImportUtilities.SplitName(r.Name);
                    return new { FirstName, LastName, serviceDate, facilityId };
                });

                OnProgressChanged(new WorkerProgressEventArgs
                {
                    JobId = jobId,
                    Stage = _stageProcessingRecords,
                    TotalItems = totalRecords,
                    ProcessedItems = totalRecords,
                    CurrentOperation = "All records processed"
                });

                // Save to database
                OnProgressChanged(new WorkerProgressEventArgs
                {
                    JobId = jobId,
                    Stage = _stageSavingData,
                    TotalItems = totalRecords,
                    ProcessedItems = totalRecords,
                    CurrentOperation = "Saving data to database"
                });

                await BulkInsertNewOnlyAsync(recordList).ConfigureAwait(false);

                // Report completion with additional data
                OnProgressChanged(new WorkerProgressEventArgs
                {
                    JobId = jobId,
                    Stage = _stageCompleted,
                    TotalItems = totalRecords,
                    ProcessedItems = totalRecords,
                    CurrentOperation = $"Import completed successfully. {totalRecords} records imported.",
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["FileName"] = Path.GetFileName(filePath),
                        ["ServiceDate"] = serviceDate.ToString("yyyy-MM-dd"),
                        ["FacilityId"] = facilityId,
                        ["RecordsImported"] = totalRecords
                    }
                });
            }
            catch (Exception ex)
            {
                OnProgressChanged(new WorkerProgressEventArgs
                {
                    JobId = jobId,
                    Stage = _stageFailed,
                    CurrentOperation = $"Import failed: {ex.Message}",
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["ErrorMessage"] = ex.Message,
                        ["ErrorType"] = ex.GetType().Name
                    }
                });

                throw;
            }
        }

        protected virtual void OnProgressChanged(WorkerProgressEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        private static object? ConvertToTargetType(object source, Type targetType)
        {
            var sourceType = source.GetType();
            var underlyingTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // If already assignable, return directly
            if (underlyingTarget.IsAssignableFrom(sourceType))
            {
                return source;
            }

            // DateOnly <-> DateTime conversions (common when caller passes DateOnly)
            if (source is DateOnly dOnly)
            {
                if (underlyingTarget == typeof(DateTime))
                {
                    return dOnly.ToDateTime(TimeOnly.MinValue);
                }
                if (underlyingTarget == typeof(DateOnly))
                {
                    return dOnly;
                }
                // if target is string
                if (underlyingTarget == typeof(string))
                {
                    return dOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
            }

            if (source is DateTime dt)
            {
                if (underlyingTarget == typeof(DateOnly))
                {
                    return DateOnly.FromDateTime(dt);
                }
                if (underlyingTarget == typeof(string))
                {
                    return dt.ToString("o", CultureInfo.InvariantCulture);
                }
            }

            // string -> DateOnly/DateTime
            if (source is string s)
            {
                if (underlyingTarget == typeof(DateOnly) && DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dd))
                {
                    return dd;
                }
                if (underlyingTarget == typeof(DateTime) && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dtt))
                {
                    return dtt;
                }
            }

            // Try general conversion for primitives / enums
            try
            {
                if (underlyingTarget.IsEnum)
                {
                    if (source is string ss)
                    {
                        return Enum.Parse(underlyingTarget, ss, ignoreCase: true);
                    }

                    return Enum.ToObject(underlyingTarget, source);
                }

                return Convert.ChangeType(source, underlyingTarget, CultureInfo.InvariantCulture);
            }
            catch
            {
                // Last resort attempts: try JSON-like conversions for common cases
                // If conversion fails, throw a clear exception
                throw new InvalidCastException($"Unable to convert value of type '{sourceType.FullName}' to '{targetType.FullName}'. Source value: '{source}'.");
            }
        }

        private static DataTable CreateAssignmentsDataTable(IEnumerable<PatientData> patients)
        {
            var dt = new DataTable();

            // Columns must match the TVP definition (names and compatible types)
            dt.Columns.Add("CaseId", typeof(string));
            dt.Columns.Add("FirstName", typeof(string));
            dt.Columns.Add("LastName", typeof(string));
            dt.Columns.Add("Mrn", typeof(string));
            dt.Columns.Add("Sex", typeof(string));
            dt.Columns.Add("Dob", typeof(DateTime)); // nullable allowed via DBNull
            dt.Columns.Add("DateServiced", typeof(DateTime)); // map from Doa if needed
            dt.Columns.Add("Room", typeof(string));
            dt.Columns.Add("Bed", typeof(string));
            dt.Columns.Add("Doa", typeof(DateTime)); // optional duplicate column if your SP expects it
            dt.Columns.Add("Los", typeof(int));
            dt.Columns.Add("AttendingPhysician", typeof(string));
            dt.Columns.Add("PrimaryInsurance", typeof(string));
            dt.Columns.Add("AdmDx", typeof(string));
            dt.Columns.Add("FacilityId", typeof(int));

            foreach (var p in patients)
            {
                var row = dt.NewRow();
                row["CaseId"] = p.CaseId ?? (object)DBNull.Value;
                row["FirstName"] = p.FirstName!;
                row["LastName"] = p.LastName!;
                row["Mrn"] = p.Mrn ?? (object)DBNull.Value;
                row["Sex"] = (p.Sex ?? (object)DBNull.Value);
                row["Dob"] = p.Dob.HasValue ? (object)p.Dob.Value : DBNull.Value;
                // DateServiced: your SP uses ISNULL(r.DateServiced, r.Doa) so populate DateServiced if you have one, otherwise use Doa
                row["DateServiced"] = p.ServiceDate!;
                row["Room"] = (p.Room ?? (object)DBNull.Value);
                row["Bed"] = (p.Bed ?? (object)DBNull.Value);
                row["Doa"] = p.Doa != default ? (object)p.Doa : DBNull.Value;
                row["Los"] = p.Los;
                row["AttendingPhysician"] = (p.AttendingPhysician ?? (object)DBNull.Value);
                row["PrimaryInsurance"] = (p.PrimaryInsurance ?? (object)DBNull.Value);
                row["AdmDx"] = (p.AdmDx ?? (object)DBNull.Value);
                row["FacilityId"] = p.FacilityId;

                dt.Rows.Add(row);
            }

            return dt;
        }

        private static void FillFields<T>(IEnumerable<PatientData> records, Func<PatientData, T> valueFactory)
        {
            ArgumentNullException.ThrowIfNull(records);
            ArgumentNullException.ThrowIfNull(valueFactory);
            foreach (var record in records)
            {
                var value = valueFactory(record);
                if (value == null)
                {
                    continue;
                }
                var valueType = value.GetType();
                var valueProps = valueType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                if (valueProps.Length == 0)
                {
                    continue;
                }
                // Build a lookup of PatientData properties by name (case-insensitive)
                var targetType = typeof(PatientData);
                var targetProps = targetType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
                foreach (var vp in valueProps)
                {
                    if (!targetProps.TryGetValue(vp.Name, out var tp))
                    {
                        // try also: strip common suffixes/prefixes? (not required here)
                        continue;
                    }
                    if (!tp.CanWrite)
                    {
                        continue;
                    }
                    var sourceValue = vp.GetValue(value);
                    if (sourceValue == null)
                    {
                        // only set null if target accepts it
                        if (IsNullable(tp.PropertyType))
                        {
                            tp.SetValue(record, null);
                        }
                        continue;
                    }
                    var converted = ConvertToTargetType(sourceValue, tp.PropertyType);
                    tp.SetValue(record, converted);
                }
            }
        }
        private static bool IsNullable(Type t)
        {
            return !t.IsValueType || Nullable.GetUnderlyingType(t) != null;
        }

        private async Task BulkInsertNewOnlyAsync(IEnumerable<PatientData> patients)
        {
            ArgumentNullException.ThrowIfNull(patients);

            var list = patients.ToList();
            if (list.Count == 0)
            {
                return;
            }

            var table = CreateAssignmentsDataTable(list);

            // Use the DbContext connection. Ensure it is a SqlConnection at runtime.
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "dbo.ImportAssignments";
            cmd.CommandType = CommandType.StoredProcedure;

            var param = new SqlParameter("@Rows", SqlDbType.Structured)
            {
                TypeName = "dbo.Assignments", // <-- must match the TVP type in the database
                Value = table
            };
            cmd.Parameters.Add(param);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
