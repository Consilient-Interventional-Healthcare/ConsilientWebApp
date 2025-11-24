using ClosedXML.Excel;
using Consilient.Infrastructure.ExcelImporter.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using static Consilient.Infrastructure.ExcelImporter.ExcelImporter;

namespace Consilient.Infrastructure.ExcelImporter.Tests
{
    [TestClass]
    public class ExcelImporterTests
    {
        public TestContext TestContext { get; set; } = null!;

        private readonly IEnumerable<string> _worksheetNames = [
          "PatientAdmissionReportXLS",
          "Joseph 100",
          "Torrico 300",
          "Jordan 300",
          "Kletz 400",
          "Kristin 400",
          "Kletz 500",
          "Connor 500",
          "Kaula 600",
          "Kletz 700"
        ];
        private readonly IEnumerable<string> _regexPatterns = ["^(Joseph|Torrico|Jordan) \\d+$"];

        [TestMethod]
        public void FilterWorksheets_ShouldReturnFilteredWorksheets()
        {
            // Arrange
            var mockWorksheets = new Mock<IXLWorksheets>();
            var allSheets = new List<IXLWorksheet>();

            foreach (var name in _worksheetNames)
            {
                var mockSheet = new Mock<IXLWorksheet>();
                mockSheet.Setup(s => s.Name).Returns(name);
                allSheets.Add(mockSheet.Object);
            }

            mockWorksheets.As<IEnumerable<IXLWorksheet>>().Setup(m => m.GetEnumerator()).Returns(allSheets.GetEnumerator());

            var expectedFilteredNames = new List<string> {
                "Joseph 100",
                "Torrico 300",
                "Jordan 300"
            };

            // Act
            var filteredSheets = ExcelImporter.FilterWorksheets(mockWorksheets.Object, [.. _regexPatterns]);
            var filteredSheetNames = filteredSheets.Select(ws => ws.Name).ToList();

            // Assert
            Assert.HasCount(expectedFilteredNames.Count, filteredSheetNames);
            CollectionAssert.AreEquivalent(expectedFilteredNames, filteredSheetNames);
        }

        [TestMethod]
        public void Import_ShouldReturnExpectedData()
        {
            // Arrange
            const string filePath = @"Files\DoctorAssignments.xls";
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            var excelImporter = new ExcelImporter(
                new ExcelImporterConfiguration
                {
                    CanConvertFile = true,
                    WorksheetFilters = _regexPatterns
                },
                loggerFactory.CreateLogger<ExcelImporter>()
            );

            // Act
            var result = excelImporter.Import(filePath).ToList();

            // Write result to a CSV file
            var outputDirectory = Directory.GetParent(TestContext.TestRunDirectory!)!.FullName;
            var outputFilePath = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(filePath)}_{Guid.NewGuid()}_output.csv");
            CsvTestHelper.WriteToCsv(result, outputFilePath);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(30, result);
            var first = result.First();
            var patientData = new PatientData
            {
                CaseId = "2404805",
                Name = "Smith, Liam",
                Mrn = "124461",
                Sex = "Female",
                Age = 77,
                Dob = new DateTime(1988, 5, 21),
                Room = "101",
                Bed = "A",
                Doa = new DateTime(2024, 12, 19, 12, 6, 0),
                Los = 301,
                AttendingPhysician = "Torrico, Tyler",
                PrimaryInsurance = "HUMANA",
                AdmDx = "F29 - Unspecified psychosis not due to a substance or known physiological condition"
            };

            Assert.AreEqual(patientData.CaseId, first.CaseId);
            Assert.AreEqual(patientData.Name, first.Name);
            Assert.AreEqual(patientData.Mrn, first.Mrn);
            Assert.AreEqual(patientData.Sex, first.Sex);
            Assert.AreEqual(patientData.Age, first.Age);
            Assert.AreEqual(patientData.Dob, first.Dob);
            Assert.AreEqual(patientData.Room, first.Room);
            Assert.AreEqual(patientData.Bed, first.Bed);
            Assert.AreEqual(patientData.Doa, first.Doa);
            Assert.AreEqual(patientData.Los, first.Los);
            Assert.AreEqual(patientData.AttendingPhysician, first.AttendingPhysician);
            Assert.AreEqual(patientData.PrimaryInsurance, first.PrimaryInsurance);
            Assert.AreEqual(patientData.AdmDx, first.AdmDx);
        }

        [TestMethod]
        public void ImportAssignments_ShouldReturnExpectedData()
        {
            // Arrange
            const string filePath = @"Files\DoctorAssignment_SAMPLE.xlsm";
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            var excelImporter = new AssignmentImporter(
                new ExcelImporterConfiguration
                {
                    CanConvertFile = true,
                    WorksheetFilters = _regexPatterns
                },
                loggerFactory.CreateLogger<ExcelImporter>()
            );

            // Act
            var result = excelImporter.Import(filePath).ToList();

            // Write result to a CSV file
            var outputDirectory = Directory.GetParent(TestContext.TestRunDirectory!)!.FullName;
            var outputFilePath = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(filePath)}_{Guid.NewGuid()}_output.csv");
            CsvTestHelper.WriteToCsv(result, outputFilePath);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(110, result);
            var first = result.First();
            Assert.AreEqual("a", first.Name);
            Assert.AreEqual("101A", first.Location);
            Assert.AreEqual("2504156", first.HospitalNumber);
            Assert.AreEqual(new DateTime(2025, 11, 9, 11, 14, 0), first.Admit);
            Assert.AreEqual(9, first.LOS);
            Assert.AreEqual("Complete", first.PsychEval);
            Assert.AreEqual("Dr Torrico (801) 682-6148", first.AttendingMD);
            Assert.IsTrue(string.IsNullOrWhiteSpace(first.MedicallyCleared));
            Assert.AreEqual("NP Kristin (650) 257-0484", first.NursePractitioner);
            Assert.AreEqual("YOLO COUNTY SHORT DOYLE", first.Insurance);
        }
    }
}