using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Domain;
using Consilient.Infrastructure.ExcelImporter.Mappers;
using Consilient.Infrastructure.ExcelImporter.Models;
using Consilient.Infrastructure.ExcelImporter.Readers;
using Consilient.Infrastructure.ExcelImporter.Sinks;
using Consilient.Infrastructure.ExcelImporter.Transformers;
using Consilient.Infrastructure.ExcelImporter.Validators;
using Microsoft.Extensions.Logging;

namespace Consilient.Infrastructure.ExcelImporter.Factories
{
    public class ImporterFactory(ILoggerFactory loggerFactory) : IImporterFactory
    {
        public IExcelImporter<DoctorAssignment> Create(string connectionString, int facilityId, DateOnly serviceDate)
        {
            return CreateWithSink(facilityId, serviceDate, CreateSink(connectionString));
        }

        public IExcelImporter<DoctorAssignment> CreateWithSink(int facilityId, DateOnly serviceDate, IDataSink sink)
        {
            ArgumentNullException.ThrowIfNull(sink, nameof(sink));
            // Create transformers with parameters
            var transformers = new List<IRowTransformer<DoctorAssignment>>
            {
                new TrimStringsTransformer<DoctorAssignment>(),
                new SetImportParametersTransformer(facilityId, serviceDate)
            };

            var options = CreateDefaultImportOptions();

            // Create importer
            var importer = new ExcelImporter<DoctorAssignment>(
                new NpoiExcelReader(),
                new ReflectionRowMapper<DoctorAssignment>(loggerFactory.CreateLogger<ReflectionRowMapper<DoctorAssignment>>()),
                new[] { new DoctorAssignmentValidator() },
                transformers,
                sink,
                options,
                loggerFactory.CreateLogger<ExcelImporter<DoctorAssignment>>());

            return importer;
        }

        private static ColumnMapping CreateColumnMapping()
        {
            return ColumnMapping.Builder()
                .MapRequired("Name↓", nameof(DoctorAssignment.Name))
                .MapRequired("Location", nameof(DoctorAssignment.Location))
                .MapRequired("Hospital Number", nameof(DoctorAssignment.HospitalNumber))
                .MapRequired("Admit", nameof(DoctorAssignment.Admit))
                .MapRequired("MRN", nameof(DoctorAssignment.Mrn))
                .Map("Age", nameof(DoctorAssignment.Age))
                .Map("DOB", nameof(DoctorAssignment.Dob))
                .Map("H&P", nameof(DoctorAssignment.H_P))
                .Map("Psych Eval", nameof(DoctorAssignment.PsychEval))
                .Map("Attending MD", nameof(DoctorAssignment.AttendingMD))
                .Map("Cleared", nameof(DoctorAssignment.IsCleared))
                .Map("Nurse Practitioner", nameof(DoctorAssignment.NursePractitioner))
                .Map("Insurance", nameof(DoctorAssignment.Insurance))
                .Build();
        }

        private static ImportOptions CreateDefaultImportOptions()
        {
            return new ImportOptions
            {
                Sheet = SheetSelector.FirstSheet,
                ColumnMapping = CreateColumnMapping(),
                BatchSize = 1000,
                FailOnValidationError = false,
                SkipEmptyRows = true,
                MaxRows = int.MaxValue
            };
        }

        private static SqlServerBulkSink CreateSink(string connectionString)
        {
            return new SqlServerBulkSink(connectionString, "staging.DoctorAssignments");
        }
    }
}
