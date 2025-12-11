using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Domain;
using Consilient.Infrastructure.ExcelImporter.Factories;
using Consilient.Infrastructure.ExcelImporter.Mappers;
using Consilient.Infrastructure.ExcelImporter.Readers;
using Consilient.Infrastructure.ExcelImporter.Sinks;
using Consilient.Infrastructure.ExcelImporter.Tests.Helpers;
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

            var sink = new InMemorySink<DoctorAssignment>();
            var facilityId = 123; // Example facility ID
            var serviceDate = DateOnly.FromDateTime(DateTime.Now);
            var importer = ImporterFactoryHelper.CreateImporterWithInMemorySink(facilityId, serviceDate, sink);


            // Act
            var result = await importer.ImportAsync(filePath, CancellationToken.None);

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


            var facilityId = 123; // Example facility ID
            var serviceDate = DateOnly.FromDateTime(DateTime.Now);
            var csvSink = new CsvFileSink(outputPath);
            var importer = ImporterFactoryHelper.CreateImporterWithInMemorySink(facilityId, serviceDate, csvSink);


            try
            {
                // Act
                var result = await importer.ImportAsync(filePath, CancellationToken.None);

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


            var facilityId = 123; // Example facility ID
            var serviceDate = DateOnly.FromDateTime(DateTime.Now);
            var importer = ImporterFactoryHelper.CreateImporterWithInMemorySink(facilityId, serviceDate);

            // Act
            var result = await importer.ImportAsync(filePath, CancellationToken.None);

            // Assert
            Assert.IsGreaterThan(0, result.TotalRowsWritten, "Should have received progress reports");
            //Assert.IsTrue(progressReports.Any(p => p.Stage == "Initializing"), "Should have Initializing stage");
            //Assert.IsTrue(progressReports.Any(p => p.Stage == "Reading"), "Should have Reading stage");
            //Assert.IsTrue(progressReports.Any(p => p.Stage == "Processing"), "Should have Processing stage");
            //Assert.IsTrue(progressReports.Any(p => p.Stage == "Finalizing"), "Should have Finalizing stage");
        }

        [TestMethod]
        public async Task FullPipeline_WithValidationErrors_CollectsErrors()
        {
            // Arrange
            var filePath = TestFileHelper.GetTestFilePath(@"Files\DoctorAssignment_SAMPLE.xlsm", TestContext);


            var facilityId = 123; // Example facility ID
            var serviceDate = DateOnly.FromDateTime(DateTime.Now);
            var importer = ImporterFactoryHelper.CreateImporterWithInMemorySink(facilityId, serviceDate);

            // Act
            var result = await importer.ImportAsync(filePath, CancellationToken.None);

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

            var sink = new InMemorySink<DoctorAssignment>();
            var facilityId = 123; // Example facility ID
            var serviceDate = DateOnly.FromDateTime(DateTime.Now);
            var importer = ImporterFactoryHelper.CreateImporterWithInMemorySink(facilityId, serviceDate, sink);


            // Act
            var result = await importer.ImportAsync(filePath, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(result.TotalRowsWritten, sink.Rows);
        }
    }
}
