using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Services;
using Consilient.Infrastructure.ExcelImporter.Models;
using Consilient.Infrastructure.ExcelImporter.Readers;
using Consilient.Infrastructure.ExcelImporter.Sinks;
using Consilient.Infrastructure.ExcelImporter.Tests.Helpers;

namespace Consilient.Infrastructure.ExcelImporter.Tests
{
    [TestClass]
    [DeploymentItem("Files\\DoctorAssignment_SAMPLE.xlsm")]
    public class ExcelImporterTests
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public async Task ExcelReader_ReadRows_ShouldReturnExpectedData()
        {
            // Arrange
            var filePath = TestFileHelper.GetTestFilePath(@"Files\DoctorAssignment_SAMPLE.xlsm", TestContext);
            var reader = new NpoiExcelReader();
            var sheetSelector = SheetSelector.FirstSheet;

            // Act
            var rows = new List<ExcelRow>();
            await using var fileStream = File.OpenRead(filePath);
            await foreach (var row in reader.ReadRowsAsync(fileStream, sheetSelector, CancellationToken.None))
            {
                rows.Add(row);
            }

            // Assert
            Assert.IsGreaterThan(0, rows.Count, "No rows were read from the Excel file");

            var firstRow = rows[0];
            Assert.AreEqual("Wymer, Mias", firstRow.Cells["Name↓"]);
            Assert.AreEqual("101A", firstRow.Cells["Location"]);
            Assert.AreEqual("2504322", firstRow.Cells["Hospital Number"]);
            Assert.AreEqual("127116", firstRow.Cells["MRN"]);
        }

        [TestMethod]
        public async Task Import_EndToEnd_ShouldWriteToCsv()
        {
            // Arrange
            var filePath = TestFileHelper.GetTestFilePath(@"Files\DoctorAssignment_SAMPLE.xlsm", TestContext);
            var outputFilePath = TestFileHelper.CreateOutputFilePathFromInput(filePath, TestContext, suffix: "output", extension: ".csv");

            var facilityId = 123; // Example facility ID
            var serviceDate = DateOnly.FromDateTime(DateTime.Now);
            var csvSink = new CsvFileSink(outputFilePath);
            var importer = ImporterFactoryHelper.CreateImporter(new TrivialSinkProvider(csvSink), facilityId, serviceDate);

            try
            {
                // Act
                await using var stream = File.OpenRead(filePath);
                var batchId = Guid.NewGuid();
                var result = await importer.ImportAsync(batchId, stream, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsGreaterThan(0, result.TotalRowsRead, "Should have read at least one row");
                Assert.IsGreaterThan(0, result.TotalRowsWritten, "Should have written at least one row");
                Assert.IsTrue(File.Exists(outputFilePath), "CSV output file was not created");

                var lines = File.ReadAllLines(outputFilePath);
                Assert.IsGreaterThan(1, lines.Length, "CSV should have header + data rows");

                // Verify header
                Assert.Contains("Name", lines[0], "Header should contain 'Name'");

                // Verify first data row contains expected values
                Assert.Contains("Wymer, Mias", lines[1], "First data row should contain patient name");
            }
            finally
            {
                if (File.Exists(outputFilePath))
                    File.Delete(outputFilePath);
            }
        }

        [TestMethod]
        public async Task CsvSink_CreatesFileWithHeaderAndRows()
        {
            // Arrange
            var outputFilePath = TestFileHelper.CreateOutputFilePathFromInput("test.csv", TestContext, "csv-writer-test");

            var patients = new List<ExternalProviderAssignment>
            {
                new() {
                    HospitalNumber = "2504322",
                    Name = "Wymer, Mias",
                    Mrn = "127116",
                    Age = 18,
                    Dob = new DateOnly(2007, 10, 13),
                    Location = "101A",
                    Admit = new DateTime(2025, 11, 22, 19, 49, 0),
                    AttendingMD = "Dr Hasija (201) 286-3100",
                    Insurance = "I/P MEDI CAL SONOMA COUNTY",
                    IsCleared = "Yes",
                    NursePractitioner = "NP Genevie (702) 497-7946",
                    H_P = "Complete",
                    PsychEval = "Complete"
                }
            };

            var sink = new CsvFileSink(outputFilePath);

            try
            {
                // Act
                var batchId = Guid.NewGuid();
                await sink.InitializeAsync(TestContext.CancellationToken);
                await sink.WriteBatchAsync(batchId, patients, TestContext.CancellationToken);
                await sink.FinalizeAsync(TestContext.CancellationToken);

                // Assert
                Assert.IsTrue(File.Exists(outputFilePath), "CSV output file was not created");

                var lines = File.ReadAllLines(outputFilePath);
                Assert.IsGreaterThanOrEqualTo(2, lines.Length, "Expected header + at least one data row");

                // Verify header exists
                var header = lines[0];
                Assert.Contains("Name", header, "Header should contain 'Name'");
                Assert.Contains("Age", header, "Header should contain 'Age'");

                // Verify data row contains expected values
                var dataRow = lines[1];
                Assert.Contains("Wymer, Mias", dataRow, "Data row should contain patient name");
                Assert.Contains("18", dataRow, "Data row should contain age");
                Assert.Contains("2504322", dataRow, "Data row should contain hospital number");
            }
            finally
            {
                if (File.Exists(outputFilePath))
                    File.Delete(outputFilePath);
            }
        }


    }
}
