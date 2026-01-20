using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Unit
{
    [TestClass]
    [DeploymentItem("Files\\DoctorAssignment_SAMPLE.xlsm")]
    public class NpoiExcelReaderTests
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public async Task ReadRowsAsync_WithValidFile_ReturnsRows()
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
            Assert.IsNotEmpty(rows, "No rows were read from the Excel file");
        }

        [TestMethod]
        public async Task ReadRowsAsync_FirstRow_ContainsExpectedData()
        {
            // Arrange
            var filePath = TestFileHelper.GetTestFilePath(@"Files\DoctorAssignment_SAMPLE.xlsm", TestContext);
            var reader = new NpoiExcelReader();
            var sheetSelector = SheetSelector.FirstSheet;

            // Act
            ExcelRow? firstRow = null;
            await using var fileStream = File.OpenRead(filePath);
            await foreach (var row in reader.ReadRowsAsync(fileStream, sheetSelector, CancellationToken.None))
            {
                firstRow = row;
                break;
            }

            // Assert
            Assert.IsNotNull(firstRow, "No rows returned from Excel file");
            Assert.IsNotEmpty(firstRow.Cells, "First row has no cells");

            // Check for expected columns
            Assert.IsTrue(firstRow.Cells.ContainsKey("Name↓"), "Missing 'Name↓' column");
            Assert.IsTrue(firstRow.Cells.ContainsKey("Hospital Number"), "Missing 'Hospital Number' column");
            Assert.IsTrue(firstRow.Cells.ContainsKey("MRN"), "Missing 'MRN' column");

            // Check expected values
            Assert.AreEqual("Wymer, Mias", firstRow.Cells["Name↓"]);
            Assert.AreEqual("101A", firstRow.Cells["Location"]);
            Assert.AreEqual("2504322", firstRow.Cells["Hospital Number"]);
        }

        [TestMethod]
        public async Task ReadRowsAsync_SkipsEmptyRows()
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

                // Verify no row is completely empty
                Assert.IsTrue(row.Cells.Values.Any(v => !string.IsNullOrWhiteSpace(v)),
                    $"Row {row.RowNumber} appears to be empty but was not skipped");
            }
        }

        [TestMethod]
        public async Task ReadRowsAsync_WithCancellation_StopsReading()
        {
            // Arrange
            var filePath = TestFileHelper.GetTestFilePath(@"Files\DoctorAssignment_SAMPLE.xlsm", TestContext);
            var reader = new NpoiExcelReader();
            var sheetSelector = SheetSelector.FirstSheet;
            var cts = new CancellationTokenSource();

            // Act & Assert
            var rowCount = 0;
            var cancelled = false;
            await using var fileStream = File.OpenRead(filePath);

            try
            {
                await foreach (var row in reader.ReadRowsAsync(fileStream, sheetSelector, cts.Token))
                {
                    rowCount++;
                    if (rowCount == 2)
                    {
                        cts.Cancel();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            Assert.IsTrue(cancelled, "Should have thrown OperationCanceledException");
            Assert.AreEqual(2, rowCount, "Should have read exactly 2 rows before cancellation");
        }

        [TestMethod]
        public async Task ReadRowsAsync_WithSheetByIndex_ReadsCorrectSheet()
        {
            // Arrange
            var filePath = TestFileHelper.GetTestFilePath(@"Files\DoctorAssignment_SAMPLE.xlsm", TestContext);
            var reader = new NpoiExcelReader();
            var sheetSelector = SheetSelector.ByIndex(0);

            // Act
            ExcelRow? firstRow = null;
            await using var fileStream = File.OpenRead(filePath);
            await foreach (var row in reader.ReadRowsAsync(fileStream, sheetSelector, CancellationToken.None))
            {
                firstRow = row;
                break;
            }

            // Assert
            Assert.IsNotNull(firstRow);
            Assert.IsNotEmpty(firstRow.Cells);
        }
    }
}
