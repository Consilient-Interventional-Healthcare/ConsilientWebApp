using Consilient.Infrastructure.ExcelImporter;
using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Mappers;
using Consilient.Infrastructure.ExcelImporter.Readers;
using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;
using Consilient.ProviderAssignments.Services.Import.Transformers;
using Consilient.ProviderAssignments.Services.Import.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Consilient.ProviderAssignments.Services.Import
{
    public class ImporterFactory(
        ILoggerFactory loggerFactory,
        ISinkProvider sinkProvider,
        IValidatorProvider validatorProvider,
        IOptions<ImportSettings> settings) : IImporterFactory
    {
        public IProviderAssignmentsImporter Create(int facilityId, DateOnly serviceDate)
        {
            var sink = sinkProvider.GetSink();

            // Create enricher that transforms raw Excel rows to processed assignments
            var enricher = new ExcelToProcessedTransformer(facilityId, serviceDate);

            // Get validators directly - they now implement IRowValidator<ExcelProviderAssignmentRow>
            var validators = validatorProvider.GetValidators();

            var options = CreateDefaultImportOptions();

            // Create staged importer: Excel -> Raw -> Validated -> Processed
            var excelImporter = new StagedExcelImporter<ExcelProviderAssignmentRow, ProcessedProviderAssignment>(
                new NpoiExcelReader(),
                new ReflectionRowMapper<ExcelProviderAssignmentRow>(loggerFactory.CreateLogger<ReflectionRowMapper<ExcelProviderAssignmentRow>>()),
                validators,
                enricher,
                sink,
                options,
                loggerFactory.CreateLogger<StagedExcelImporter<ExcelProviderAssignmentRow, ProcessedProviderAssignment>>());

            return new ProviderAssignmentsImporter(excelImporter);
        }

        /// <summary>
        /// Creates column mapping for raw Excel fields only.
        /// Derived fields (Room, Bed, Normalized*) are computed by the enricher, not mapped from Excel.
        /// </summary>
        private static ColumnMapping CreateColumnMapping()
        {
            return ColumnMapping.Builder()
                .MapRequired("Name↓", nameof(ExcelProviderAssignmentRow.Name))
                .MapRequired("Location", nameof(ExcelProviderAssignmentRow.Location))
                .MapRequired("Hospital Number", nameof(ExcelProviderAssignmentRow.HospitalNumber))
                .MapRequired("Admit", nameof(ExcelProviderAssignmentRow.Admit))
                .MapRequired("MRN", nameof(ExcelProviderAssignmentRow.Mrn))
                .Map("Age", nameof(ExcelProviderAssignmentRow.Age))
                .Map("DOB", nameof(ExcelProviderAssignmentRow.Dob))
                .Map("H&P", nameof(ExcelProviderAssignmentRow.H_P))
                .Map("Psych Eval", nameof(ExcelProviderAssignmentRow.PsychEval))
                .Map("Attending MD", nameof(ExcelProviderAssignmentRow.AttendingMD))
                .Map("Cleared", nameof(ExcelProviderAssignmentRow.IsCleared))
                .Map("Nurse Practitioner", nameof(ExcelProviderAssignmentRow.NursePractitioner))
                .Map("Insurance", nameof(ExcelProviderAssignmentRow.Insurance))
                .Build();
        }

        private ImportOptions CreateDefaultImportOptions()
        {
            return new ImportOptions
            {
                Sheet = SheetSelector.FirstSheet,
                ColumnMapping = CreateColumnMapping(),
                BatchSize = settings.Value.BatchSize,
                FailOnValidationError = false,
                SkipEmptyRows = true,
                MaxRows = int.MaxValue
            };
        }
    }
}
