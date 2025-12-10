using Consilient.Infrastructure.ExcelImporter.Tests.Helpers;
using Microsoft.Extensions.Logging;
using static Consilient.Infrastructure.ExcelImporter.DoctorAssignmentImporter;
using NPOI.SS.UserModel;

namespace Consilient.Infrastructure.ExcelImporter.Tests
{
    [TestClass]
    public class ExcelImporterTests
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public void Import_ShouldReturnExpectedData()
        {
            // Arrange
            const string filePath = @"Files\DoctorAssignment_SAMPLE.xlsm";
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            var excelImporter = new DoctorAssignmentImporter(
                loggerFactory.CreateLogger<DoctorAssignmentImporter>()
            );

            // Act
            var result = excelImporter.Import(filePath).ToList();

            // Write result to a CSV file
            var outputDirectory = Directory.GetParent(TestContext.TestRunDirectory!)!.FullName;
            var outputFilePath = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(filePath)}_{Guid.NewGuid()}_output.csv");
            CsvTestHelper.WriteToCsv(result, outputFilePath);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(114, result);
            var first = result.First();
            var patientData = new PatientData
            {
                CaseId = "2504322",
                Name = "Wymer, Mias",
                Mrn = "127116",
                Sex = "Male",
                Age = 18,
                Dob = new DateTime(2007, 10, 13),
                Room = "101",
                Bed = "A",
                Doa = new DateTime(2025, 11, 22, 19, 49, 0),
                Los = 14,
                AttendingPhysician = "Dr Hasija (201) 286-3100",
                PrimaryInsurance = "I/P MEDI CAL SONOMA COUNTY",
                AdmDx = "Complete"
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
    }
}