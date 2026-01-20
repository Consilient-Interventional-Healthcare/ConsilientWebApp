using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Services;
using Consilient.Infrastructure.ExcelImporter.Sinks;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Unit
{
    [TestClass]
    public class SinkTests
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public async Task InMemorySink_StoresAllRows()
        {
            // Arrange
            var sink = new InMemorySink<ExcelProviderAssignmentRow>();
            var patients = new List<ExcelProviderAssignmentRow>
            {
                new() { Name = "Patient 1", HospitalNumber = "001", Mrn = "M001", Age = 25, Admit = DateTime.Now },
                new() { Name = "Patient 2", HospitalNumber = "002", Mrn = "M002", Age = 30, Admit = DateTime.Now },
                new() { Name = "Patient 3", HospitalNumber = "003", Mrn = "M003", Age = 35, Admit = DateTime.Now }
            };

            // Act
            var batchId = Guid.NewGuid();
            await sink.InitializeAsync(TestContext.CancellationToken);
            await sink.WriteBatchAsync(batchId, patients, TestContext.CancellationToken);
            await sink.FinalizeAsync(TestContext.CancellationToken);

            // Assert
            Assert.HasCount(3, sink.Rows);
            Assert.AreEqual("Patient 1", sink.Rows[0].Name);
            Assert.AreEqual("Patient 2", sink.Rows[1].Name);
            Assert.AreEqual("Patient 3", sink.Rows[2].Name);
        }

        [TestMethod]
        public async Task InMemorySink_InitializeClearsExistingRows()
        {
            // Arrange
            var batchId = Guid.NewGuid();
            var sink = new InMemorySink<ExcelProviderAssignmentRow>();
            await sink.WriteBatchAsync(batchId, new List<ExcelProviderAssignmentRow>
            {
                new() { Name = "Old Patient", HospitalNumber = "000", Mrn = "M000", Age = 20, Admit = DateTime.Now }
            }, TestContext.CancellationToken);

            // Act
            await sink.InitializeAsync(TestContext.CancellationToken); // Should clear
            await sink.WriteBatchAsync(batchId, new List<ExcelProviderAssignmentRow>
            {
                new() { Name = "New Patient", HospitalNumber = "001", Mrn = "M001", Age = 25, Admit = DateTime.Now }
            }, TestContext.CancellationToken);

            // Assert
            Assert.HasCount(1, sink.Rows);
            Assert.AreEqual("New Patient", sink.Rows[0].Name);
        }

        [TestMethod]
        public async Task CsvFileSink_CreatesFileWithData()
        {
            // Arrange
            var outputPath = TestFileHelper.CreateOutputFilePathFromInput("test.csv", TestContext, "csv-sink-test");
            var sink = new CsvFileSink(outputPath);
            var patients = new List<ExcelProviderAssignmentRow>
            {
                new() {
                    Name = "Wymer, Mias",
                    Location = "101A",
                    HospitalNumber = "2504322",
                    Mrn = "127116",
                    Age = 18,
                    Admit = new DateTime(2025, 11, 22, 19, 49, 0),
                    Dob = new DateOnly(2007, 10, 13)
                }
            };

            try
            {
                // Act
                var batchId = Guid.NewGuid();
                await sink.InitializeAsync(TestContext.CancellationToken);
                await sink.WriteBatchAsync(batchId, patients, TestContext.CancellationToken);
                await sink.FinalizeAsync(TestContext.CancellationToken);

                // Assert
                Assert.IsTrue(File.Exists(outputPath), "CSV file was not created");

                var lines = File.ReadAllLines(outputPath);
                Assert.IsGreaterThanOrEqualTo(2, lines.Length, "Expected header + at least one data row");

                // Verify header exists
                var header = lines[0];
                Assert.Contains("Name", header, "Header should contain 'Name'");
                Assert.Contains("Age", header, "Header should contain 'Age'");
                Assert.Contains("HospitalNumber", header, "Header should contain 'HospitalNumber'");

                // Verify data row exists
                var dataRow = lines[1];
                Assert.Contains("Wymer, Mias", dataRow, "Data row should contain patient name");
                Assert.Contains("18", dataRow, "Data row should contain age");
            }
            finally
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
            }
        }

        [TestMethod]
        public async Task CsvFileSink_HandlesMultipleBatches()
        {
            // Arrange
            var outputPath = TestFileHelper.CreateOutputFilePathFromInput("test.csv", TestContext, "csv-multi-batch");
            var sink = new CsvFileSink(outputPath);
            var batch1 = new List<ExcelProviderAssignmentRow>
            {
                new() { Name = "Patient 1", HospitalNumber = "001", Mrn = "M001", Age = 25, Admit = DateTime.Now }
            };
            var batch2 = new List<ExcelProviderAssignmentRow>
            {
                new() { Name = "Patient 2", HospitalNumber = "002", Mrn = "M002", Age = 30, Admit = DateTime.Now }
            };

            try
            {
                // Act
                var batchId = Guid.NewGuid();
                await sink.InitializeAsync(TestContext.CancellationToken);
                await sink.WriteBatchAsync(batchId, batch1, TestContext.CancellationToken);
                await sink.WriteBatchAsync(batchId, batch2, TestContext.CancellationToken);
                await sink.FinalizeAsync(TestContext.CancellationToken);

                // Assert
                var lines = File.ReadAllLines(outputPath);
                Assert.HasCount(3, lines, "Expected header + 2 data rows");

                Assert.Contains("Patient 1", lines[1]);
                Assert.Contains("Patient 2", lines[2]);
            }
            finally
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
            }
        }

        [TestMethod]
        public async Task CsvFileSink_HandlesEmptyBatch()
        {
            // Arrange
            var outputPath = TestFileHelper.CreateOutputFilePathFromInput("test.csv", TestContext, "csv-empty");
            var sink = new CsvFileSink(outputPath);
            var emptyBatch = new List<ExcelProviderAssignmentRow>();

            try
            {
                // Act
                var batchId = Guid.NewGuid();
                await sink.InitializeAsync(TestContext.CancellationToken);
                await sink.WriteBatchAsync(batchId, emptyBatch, TestContext.CancellationToken);
                await sink.FinalizeAsync(TestContext.CancellationToken);

                // Assert
                // File should exist but might be empty or just have header
                Assert.IsTrue(File.Exists(outputPath));
            }
            finally
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
            }
        }

        [TestMethod]
        public async Task CsvFileSink_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var outputPath = Path.Combine(tempDir, "test.csv");
            var sink = new CsvFileSink(outputPath);
            var patients = new List<ExcelProviderAssignmentRow>
            {
                new() { Name = "Test", HospitalNumber = "001", Mrn = "M001", Age = 25, Admit = DateTime.Now }
            };

            try
            {
                // Act
                var batchId = Guid.NewGuid();
                await sink.InitializeAsync(TestContext.CancellationToken);
                await sink.WriteBatchAsync(batchId, patients, TestContext.CancellationToken);
                await sink.FinalizeAsync(TestContext.CancellationToken);

                // Assert
                Assert.IsTrue(Directory.Exists(tempDir), "Directory should be created");
                Assert.IsTrue(File.Exists(outputPath), "File should be created");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}
