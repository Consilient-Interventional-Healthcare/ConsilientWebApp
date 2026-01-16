using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Services;
using Consilient.ProviderAssignments.Services.Importer;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Unit
{
    [TestClass]
    public class ProviderAssignmentValidatorTests
    {
        private ProviderAssignmentValidator _validator = null!;

        [TestInitialize]
        public void Setup()
        {
            _validator = new ProviderAssignmentValidator();
        }

        [TestMethod]
        public void Validate_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var patientData = new ExternalProviderAssignment
            {
                Name = "Wymer, Mias",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 18,
                Admit = new DateTime(2023, 11, 22),
                Location = "101A"
            };

            // Act
            var result = _validator.Validate(patientData, 1);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsEmpty(result.Errors);
        }

        [TestMethod]
        public void Validate_WithMissingName_ReturnsError()
        {
            // Arrange
            var patientData = new ExternalProviderAssignment
            {
                Name = "",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 18,
                Admit = DateTime.Now
            };

            // Act
            var result = _validator.Validate(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExternalProviderAssignment.Name)));
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("required")));
        }

        [TestMethod]
        public void Validate_WithInvalidAge_ReturnsError()
        {
            // Arrange
            var patientData = new ExternalProviderAssignment
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 200, // Invalid
                Admit = DateTime.Now
            };

            // Act
            var result = _validator.Validate(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExternalProviderAssignment.Age)));
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("0 and 150")));
        }

        [TestMethod]
        public void Validate_WithNegativeAge_ReturnsError()
        {
            // Arrange
            var patientData = new ExternalProviderAssignment
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = -5,
                Admit = DateTime.Now
            };

            // Act
            var result = _validator.Validate(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExternalProviderAssignment.Age)));
        }

        [TestMethod]
        public void Validate_WithMissingHospitalNumber_ReturnsError()
        {
            // Arrange
            var patientData = new ExternalProviderAssignment
            {
                Name = "Test Patient",
                HospitalNumber = "",
                Mrn = "127116",
                Age = 18,
                Admit = DateTime.Now
            };

            // Act
            var result = _validator.Validate(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExternalProviderAssignment.HospitalNumber)));
        }

        [TestMethod]
        public void Validate_WithFutureAdmitDate_ReturnsError()
        {
            // Arrange
            var patientData = new ExternalProviderAssignment
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 18,
                Admit = DateTime.UtcNow.AddDays(1) // Future date
            };

            // Act
            var result = _validator.Validate(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExternalProviderAssignment.Admit)));
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("future")));
        }

        [TestMethod]
        public void Validate_WithFutureDob_ReturnsError()
        {
            // Arrange
            var patientData = new ExternalProviderAssignment
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 18,
                Admit = DateTime.Now,
                Dob = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1) // Future date
            };

            // Act
            var result = _validator.Validate(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExternalProviderAssignment.Dob)));
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("future")));
        }

        [TestMethod]
        public void Validate_WithMissingMrn_ReturnsError()
        {
            // Arrange
            var patientData = new ExternalProviderAssignment
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "",
                Age = 18,
                Admit = DateTime.Now
            };

            // Act
            var result = _validator.Validate(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExternalProviderAssignment.Mrn)));
        }

        [TestMethod]
        public void Validate_WithMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var patientData = new ExternalProviderAssignment
            {
                Name = "",
                HospitalNumber = "",
                Mrn = "",
                Age = -1,
                Admit = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var result = _validator.Validate(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsGreaterThanOrEqualTo(5, result.Errors.Count, $"Expected at least 5 errors, got {result.Errors.Count}");
        }

        [TestMethod]
        public void Validate_WithValidNullDob_ReturnsSuccess()
        {
            // Arrange
            var patientData = new ExternalProviderAssignment
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 18,
                Admit = DateTime.Now,
                Dob = null
            };

            // Act
            var result = _validator.Validate(patientData, 1);

            // Assert
            Assert.IsTrue(result.IsValid);
        }
    }
}
