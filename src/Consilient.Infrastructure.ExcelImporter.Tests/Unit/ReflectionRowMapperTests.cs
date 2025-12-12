using Consilient.DoctorAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Mappers;
using Consilient.Infrastructure.ExcelImporter.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Unit
{
    [TestClass]
    public class ReflectionRowMapperTests
    {
        private ILogger<ReflectionRowMapper<ExternalDoctorAssignment>> _logger = null!;

        [TestInitialize]
        public void Setup()
        {
            _logger = NullLogger<ReflectionRowMapper<ExternalDoctorAssignment>>.Instance;
        }

        [TestMethod]
        public void Map_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var mapper = new ReflectionRowMapper<ExternalDoctorAssignment>(_logger);
            var cells = new Dictionary<string, string>
            {
                ["Name↓"] = "Wymer, Mias",
                ["Location"] = "101A",
                ["Hospital Number"] = "2504322",
                ["MRN"] = "127116",
                ["Age"] = "18",
                ["Admit"] = "2025-11-22 19:49:00"
            };
            var excelRow = new ExcelRow(1, cells);

            var columnMapping = ColumnMapping.Builder()
                .Map("Name↓", nameof(ExternalDoctorAssignment.Name))
                .Map("Location", nameof(ExternalDoctorAssignment.Location))
                .Map("Hospital Number", nameof(ExternalDoctorAssignment.HospitalNumber))
                .Map("MRN", nameof(ExternalDoctorAssignment.Mrn))
                .Map("Age", nameof(ExternalDoctorAssignment.Age))
                .Map("Admit", nameof(ExternalDoctorAssignment.Admit))
                .Build();

            // Act
            var result = mapper.Map(excelRow, columnMapping);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Wymer, Mias", result.Value.Name);
            Assert.AreEqual("101A", result.Value.Location);
            Assert.AreEqual("2504322", result.Value.HospitalNumber);
            Assert.AreEqual("127116", result.Value.Mrn);
            Assert.AreEqual(18, result.Value.Age);
        }

        [TestMethod]
        public void Map_WithMissingRequiredColumn_ReturnsFailure()
        {
            // Arrange
            var mapper = new ReflectionRowMapper<ExternalDoctorAssignment>(_logger);
            var cells = new Dictionary<string, string>
            {
                ["Location"] = "101A"
            };
            var excelRow = new ExcelRow(1, cells);

            var columnMapping = ColumnMapping.Builder()
                .MapRequired("Name↓", nameof(ExternalDoctorAssignment.Name))
                .Map("Location", nameof(ExternalDoctorAssignment.Location))
                .Build();

            // Act
            var result = mapper.Map(excelRow, columnMapping);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.Error);
            Assert.Contains("Name↓", result.Error);
            Assert.Contains("not found", result.Error);
        }

        [TestMethod]
        public void Map_WithInvalidTypeConversion_ReturnsFailure()
        {
            // Arrange
            var mapper = new ReflectionRowMapper<ExternalDoctorAssignment>(_logger);
            var cells = new Dictionary<string, string>
            {
                ["Age"] = "not a number"
            };
            var excelRow = new ExcelRow(1, cells);

            var columnMapping = ColumnMapping.Builder()
                .Map("Age", nameof(ExternalDoctorAssignment.Age))
                .Build();

            // Act
            var result = mapper.Map(excelRow, columnMapping);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.Error);
            Assert.Contains("Age", result.Error);
        }

        [TestMethod]
        public void Map_WithNullableDateTime_HandlesNull()
        {
            // Arrange
            var mapper = new ReflectionRowMapper<ExternalDoctorAssignment>(_logger);
            var cells = new Dictionary<string, string>
            {
                ["DOB"] = "",
                ["Name↓"] = "Test"
            };
            var excelRow = new ExcelRow(1, cells);

            var columnMapping = ColumnMapping.Builder()
                .Map("DOB", nameof(ExternalDoctorAssignment.Dob))
                .Map("Name↓", nameof(ExternalDoctorAssignment.Name))
                .Build();

            // Act
            var result = mapper.Map(excelRow, columnMapping);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.IsNull(result.Value.Dob);
        }

        [TestMethod]
        public void Map_WithNullableDateTime_ParsesValidDate()
        {
            // Arrange
            var mapper = new ReflectionRowMapper<ExternalDoctorAssignment>(_logger);
            var expectedDate = new DateTime(2007, 10, 13);
            var cells = new Dictionary<string, string>
            {
                ["DOB"] = "2007-10-13",
                ["Name↓"] = "Test"
            };
            var excelRow = new ExcelRow(1, cells);

            var columnMapping = ColumnMapping.Builder()
                .Map("DOB", nameof(ExternalDoctorAssignment.Dob))
                .Map("Name↓", nameof(ExternalDoctorAssignment.Name))
                .Build();

            // Act
            var result = mapper.Map(excelRow, columnMapping);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(expectedDate, result.Value.Dob);
        }

        [TestMethod]
        public void Map_WithDateOnly_ParsesCorrectly()
        {
            // Arrange
            var mapper = new ReflectionRowMapper<ExternalDoctorAssignment>(_logger);
            var expectedDate = new DateOnly(2025, 11, 22);
            var cells = new Dictionary<string, string>
            {
                ["ServiceDate"] = "2025-11-22",
                ["Name↓"] = "Test"
            };
            var excelRow = new ExcelRow(1, cells);

            var columnMapping = ColumnMapping.Builder()
                .Map("ServiceDate", nameof(ExternalDoctorAssignment.ServiceDate))
                .Map("Name↓", nameof(ExternalDoctorAssignment.Name))
                .Build();

            // Act
            var result = mapper.Map(excelRow, columnMapping);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(expectedDate, result.Value.ServiceDate);
        }

        [TestMethod]
        public void Map_WithUnmappedColumn_IgnoresIt()
        {
            // Arrange
            var mapper = new ReflectionRowMapper<ExternalDoctorAssignment>(_logger);
            var cells = new Dictionary<string, string>
            {
                ["Name↓"] = "Test",
                ["UnknownColumn"] = "SomeValue"
            };
            var excelRow = new ExcelRow(1, cells);

            var columnMapping = ColumnMapping.Builder()
                .Map("Name↓", nameof(ExternalDoctorAssignment.Name))
                .Build();

            // Act
            var result = mapper.Map(excelRow, columnMapping);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Test", result.Value.Name);
        }
    }
}
