using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Domain;
using Consilient.Infrastructure.ExcelImporter.Mappers;
using Consilient.Infrastructure.ExcelImporter.Models;
using Consilient.Infrastructure.ExcelImporter.Readers;
using Consilient.Infrastructure.ExcelImporter.Sinks;
using Consilient.Infrastructure.ExcelImporter.Transformers;
using Consilient.Infrastructure.ExcelImporter.Validators;
using Microsoft.Extensions.Logging.Abstractions;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Integration
{
    [TestClass]
    [DeploymentItem("Files\\DoctorAssignment_SAMPLE.xlsm")]
    public class ExcelImporterIntegrationTests
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public async Task FullPipeline_ImportExcelToInMemory_Success()
        {
            // Arrange
            var filePath = TestFileHelper.GetTestFilePath(@"Files\DoctorAssignment_SAMPLE.xlsm", TestContext);

            var reader = new NpoiExcelReader();
            var mapper = new ReflectionRowMapper<DoctorAssignment>(NullLogger<ReflectionRowMapper<DoctorAssignment>>.Instance);
            var validators = new List<IRowValidator<DoctorAssignment>> { new DoctorAssignmentValidator() };
            var transformers = new List<IRowTransformer<DoctorAssignment>>
            {
                new TrimStringsTransformer<DoctorAssignment>(),
                new CalculateAgeFromDobTransformer()
            };

            var importer = new ExcelImporter<DoctorAssignment>(
                reader,
                mapper,
                validators,
                transformers,
                NullLogger<ExcelImporter<DoctorAssignment>>.Instance);

            var sink = new InMemorySink<DoctorAssignment>();

            var options = new ImportOptions
            {
                Sheet = SheetSelector.FirstSheet,
                ColumnMapping = ColumnMapping.Builder()
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
                    .Build(),
                BatchSize = 100,
                FailOnValidationError = false
            };

            // Act
            var result = await importer.ImportAsync(filePath, sink, options, null, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsGreaterThan(0, result.TotalRowsRead, "Should have read at least one row");
            Assert.IsGreaterThan(0, result.TotalRowsWritten, "Should have written at least one row");
            Assert.IsNotEmpty(sink.Rows, "Sink should contain rows");

            // Verify first patient
            var firstPatient = sink.Rows[0];
            Assert.AreEqual("Wymer, Mias", firstPatient.Name);
            Assert.AreEqual("101A", firstPatient.Location);
            Assert.AreEqual("2504322", firstPatient.HospitalNumber);
            Assert.AreEqual("127116", firstPatient.Mrn);
        }

        [TestMethod]
        public async Task FullPipeline_ImportExcelToCsv_Success()
        {
            // Arrange
            var filePath = TestFileHelper.GetTestFilePath(@"Files\DoctorAssignment_SAMPLE.xlsm", TestContext);
            var outputPath = TestFileHelper.CreateOutputFilePathFromInput(filePath, TestContext, "integration-test");

            var reader = new NpoiExcelReader();
            var mapper = new ReflectionRowMapper<DoctorAssignment>(NullLogger<ReflectionRowMapper<DoctorAssignment>>.Instance);
            var validators = new List<IRowValidator<DoctorAssignment>> { new DoctorAssignmentValidator() };
            var transformers = new List<IRowTransformer<DoctorAssignment>>
            {
                new TrimStringsTransformer<DoctorAssignment>()
            };

            var importer = new ExcelImporter<DoctorAssignment>(
                reader,
                mapper,
                validators,
                transformers,
                NullLogger<ExcelImporter<DoctorAssignment>>.Instance);

            var sink = new CsvFileSink(outputPath);

            var options = new ImportOptions
            {
                Sheet = SheetSelector.FirstSheet,
                ColumnMapping = ColumnMapping.Builder()
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
                    .Build(),
                BatchSize = 100,
                FailOnValidationError = false
            };

            try
            {
                // Act
                var result = await importer.ImportAsync(filePath, sink, options, null, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsGreaterThan(0, result.TotalRowsRead);
                Assert.IsGreaterThan(0, result.TotalRowsWritten);
                Assert.IsTrue(File.Exists(outputPath), "CSV output file should exist");

                var lines = File.ReadAllLines(outputPath);
                Assert.IsGreaterThan(1, lines.Length, "Should have header + data rows");
            }
            finally
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
            }
        }

        [TestMethod]
        public async Task FullPipeline_WithProgressReporting_ReportsProgress()
        {
            // Arrange
            var filePath = TestFileHelper.GetTestFilePath(@"Files\DoctorAssignment_SAMPLE.xlsm", TestContext);

            var reader = new NpoiExcelReader();
            var mapper = new ReflectionRowMapper<DoctorAssignment>(NullLogger<ReflectionRowMapper<DoctorAssignment>>.Instance);
            var importer = new ExcelImporter<DoctorAssignment>(
                reader,
                mapper,
                new List<IRowValidator<DoctorAssignment>>(),
                new List<IRowTransformer<DoctorAssignment>>(),
                NullLogger<ExcelImporter<DoctorAssignment>>.Instance);

            var sink = new InMemorySink<DoctorAssignment>();

            var options = new ImportOptions
            {
                Sheet = SheetSelector.FirstSheet,
                ColumnMapping = ColumnMapping.Builder()
                    .MapRequired("Name↓", nameof(DoctorAssignment.Name))
                    .MapRequired("Hospital Number", nameof(DoctorAssignment.HospitalNumber))
                    .MapRequired("MRN", nameof(DoctorAssignment.Mrn))
                    .Build(),
                BatchSize = 2,
                FailOnValidationError = false
            };

            var progressReports = new List<ImportProgress>();
            var progress = new Progress<ImportProgress>(p => progressReports.Add(p));

            // Act
            await importer.ImportAsync(filePath, sink, options, progress, CancellationToken.None);

            // Assert
            Assert.IsGreaterThan(0, progressReports.Count, "Should have received progress reports");
            Assert.IsTrue(progressReports.Any(p => p.Stage == "Initializing"), "Should have Initializing stage");
            Assert.IsTrue(progressReports.Any(p => p.Stage == "Reading"), "Should have Reading stage");
            Assert.IsTrue(progressReports.Any(p => p.Stage == "Processing"), "Should have Processing stage");
            Assert.IsTrue(progressReports.Any(p => p.Stage == "Finalizing"), "Should have Finalizing stage");
        }

        [TestMethod]
        public async Task FullPipeline_WithValidationErrors_CollectsErrors()
        {
            // Arrange
            var filePath = TestFileHelper.GetTestFilePath(@"Files\DoctorAssignment_SAMPLE.xlsm", TestContext);

            var reader = new NpoiExcelReader();
            var mapper = new ReflectionRowMapper<DoctorAssignment>(NullLogger<ReflectionRowMapper<DoctorAssignment>>.Instance);
            var validators = new List<IRowValidator<DoctorAssignment>> { new DoctorAssignmentValidator() };

            var importer = new ExcelImporter<DoctorAssignment>(
                reader,
                mapper,
                validators,
                new List<IRowTransformer<DoctorAssignment>>(),
                NullLogger<ExcelImporter<DoctorAssignment>>.Instance);

            var sink = new InMemorySink<DoctorAssignment>();

            var options = new ImportOptions
            {
                Sheet = SheetSelector.FirstSheet,
                ColumnMapping = ColumnMapping.Builder()
                    .MapRequired("Name↓", nameof(DoctorAssignment.Name))
                    .MapRequired("Hospital Number", nameof(DoctorAssignment.HospitalNumber))
                    .MapRequired("MRN", nameof(DoctorAssignment.Mrn))
                    .Map("Age", nameof(DoctorAssignment.Age))
                    .Map("Admit", nameof(DoctorAssignment.Admit))
                    .Build(),
                BatchSize = 100,
                FailOnValidationError = false // Collect errors, don't fail
            };

            // Act
            var result = await importer.ImportAsync(filePath, sink, options, null, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            // Some rows may have validation errors, but import should complete
            Assert.IsGreaterThan(0, result.TotalRowsRead);
        }

        [TestMethod]
        public async Task FullPipeline_WithBatching_ProcessesInBatches()
        {
            // Arrange
            var filePath = TestFileHelper.GetTestFilePath(@"Files\DoctorAssignment_SAMPLE.xlsm", TestContext);

            var reader = new NpoiExcelReader();
            var mapper = new ReflectionRowMapper<DoctorAssignment>(NullLogger<ReflectionRowMapper<DoctorAssignment>>.Instance);
            var importer = new ExcelImporter<DoctorAssignment>(
                reader,
                mapper,
                new List<IRowValidator<DoctorAssignment>>(),
                new List<IRowTransformer<DoctorAssignment>>(),
                NullLogger<ExcelImporter<DoctorAssignment>>.Instance);

            var sink = new InMemorySink<DoctorAssignment>();

            var options = new ImportOptions
            {
                Sheet = SheetSelector.FirstSheet,
                ColumnMapping = ColumnMapping.Builder()
                    .MapRequired("Name↓", nameof(DoctorAssignment.Name))
                    .MapRequired("Hospital Number", nameof(DoctorAssignment.HospitalNumber))
                    .MapRequired("MRN", nameof(DoctorAssignment.Mrn))
                    .Build(),
                BatchSize = 2, // Small batch size to test batching
                FailOnValidationError = false
            };

            // Act
            var result = await importer.ImportAsync(filePath, sink, options, null, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(result.TotalRowsWritten, sink.Rows);
        }
    }
}
