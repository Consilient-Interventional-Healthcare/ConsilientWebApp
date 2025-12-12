using Consilient.Data.Entities;
using Consilient.DoctorAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Mappers;
using Consilient.Infrastructure.ExcelImporter.Models;
using Consilient.Infrastructure.ExcelImporter.Readers;
using Consilient.Infrastructure.ExcelImporter.Transformers;
using Microsoft.Extensions.Logging;

namespace Consilient.DoctorAssignments.Services.Importer
{
    public class ImporterFactory(ILoggerFactory loggerFactory, ISinkProvider sinkProvider) : IImporterFactory
    {
        public IDoctorAssignmentsImporter Create(int facilityId, DateOnly serviceDate)
        {
            var sink = sinkProvider.GetSink();
            // Create transformers with parameters
            var transformers = new List<IRowTransformer<ExternalDoctorAssignment>>
            {
                new TrimStringsTransformer<ExternalDoctorAssignment>(),
                new SetImportParametersTransformer(facilityId, serviceDate)
            };

            var options = CreateDefaultImportOptions();

            // Create importer
            var excelImporter = new ExcelImporter<ExternalDoctorAssignment>(
                new NpoiExcelReader(),
                new ReflectionRowMapper<ExternalDoctorAssignment>(loggerFactory.CreateLogger<ReflectionRowMapper<ExternalDoctorAssignment>>()),
                new[] { new DoctorAssignmentValidator() },
                transformers,
                sink,
                options,
                loggerFactory.CreateLogger<ExcelImporter<ExternalDoctorAssignment>>());


            return new DoctorAssignmentsImporter(excelImporter);
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
    }
}
