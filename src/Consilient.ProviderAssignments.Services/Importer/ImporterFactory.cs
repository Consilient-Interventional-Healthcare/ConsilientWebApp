using Consilient.Data.Entities;
using Consilient.ProviderAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Mappers;
using Consilient.Infrastructure.ExcelImporter.Models;
using Consilient.Infrastructure.ExcelImporter.Readers;
using Consilient.Infrastructure.ExcelImporter.Transformers;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Importer
{
    public class ImporterFactory(ILoggerFactory loggerFactory, ISinkProvider sinkProvider) : IImporterFactory
    {
        public IProviderAssignmentsImporter Create(int facilityId, DateOnly serviceDate)
        {
            var sink = sinkProvider.GetSink();
            // Create transformers with parameters
            var transformers = new List<IRowTransformer<ExternalProviderAssignment>>
            {
                new TrimStringsTransformer<ExternalProviderAssignment>(),
                new SetImportParametersTransformer(facilityId, serviceDate)
            };

            var options = CreateDefaultImportOptions();

            // Create importer
            var excelImporter = new ExcelImporter<ExternalProviderAssignment>(
                new NpoiExcelReader(),
                new ReflectionRowMapper<ExternalProviderAssignment>(loggerFactory.CreateLogger<ReflectionRowMapper<ExternalProviderAssignment>>()),
                new[] { new ProviderAssignmentValidator() },
                transformers,
                sink,
                options,
                loggerFactory.CreateLogger<ExcelImporter<ExternalProviderAssignment>>());


            return new ProviderAssignmentsImporter(excelImporter);
        }

        private static ColumnMapping CreateColumnMapping()
        {
            return ColumnMapping.Builder()
                .MapRequired("Name↓", nameof(ProviderAssignment.Name))
                .MapRequired("Location", nameof(ProviderAssignment.Location))
                .MapRequired("Hospital Number", nameof(ProviderAssignment.HospitalNumber))
                .MapRequired("Admit", nameof(ProviderAssignment.Admit))
                .MapRequired("MRN", nameof(ProviderAssignment.Mrn))
                .Map("Age", nameof(ProviderAssignment.Age))
                .Map("DOB", nameof(ProviderAssignment.Dob))
                .Map("H&P", nameof(ProviderAssignment.H_P))
                .Map("Psych Eval", nameof(ProviderAssignment.PsychEval))
                .Map("Attending MD", nameof(ProviderAssignment.AttendingMD))
                .Map("Cleared", nameof(ProviderAssignment.IsCleared))
                .Map("Nurse Practitioner", nameof(ProviderAssignment.NursePractitioner))
                .Map("Insurance", nameof(ProviderAssignment.Insurance))
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
