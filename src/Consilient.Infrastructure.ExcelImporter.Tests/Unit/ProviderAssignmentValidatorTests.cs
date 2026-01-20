using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Services.Import.Validation;
using Consilient.ProviderAssignments.Services.Import.Validation.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Unit
{
    [TestClass]
    public class ProviderAssignmentValidatorTests
    {
        private IEnumerable<IExcelRowValidator> _validators = null!;

        [TestInitialize]
        public void Setup()
        {
            // Create a test service provider with all validators registered
            var services = new ServiceCollection();
            services.AddScoped<IExcelRowValidator, NameRequiredValidator>();
            services.AddScoped<IExcelRowValidator, AgeRangeValidator>();
            services.AddScoped<IExcelRowValidator, HospitalNumberValidator>();
            services.AddScoped<IExcelRowValidator, DateFieldsValidator>();
            services.AddScoped<IExcelRowValidator, MrnValidator>();
            var serviceProvider = services.BuildServiceProvider();

            var validatorProvider = new ValidatorProvider(serviceProvider);
            _validators = validatorProvider.GetValidators();
        }

        /// <summary>
        /// Helper method to run all validators and aggregate results.
        /// </summary>
        private ValidationResult ValidateAll(ExcelProviderAssignmentRow row, int rowNumber)
        {
            var allErrors = new List<ValidationError>();
            foreach (var validator in _validators)
            {
                var result = validator.Validate(row, rowNumber);
                if (!result.IsValid)
                {
                    allErrors.AddRange(result.Errors);
                }
            }
            return allErrors.Count > 0
                ? ValidationResult.Failed([.. allErrors])
                : ValidationResult.Success();
        }

        [TestMethod]
        public void Validate_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var patientData = new ExcelProviderAssignmentRow
            {
                Name = "Wymer, Mias",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 18,
                Admit = new DateTime(2023, 11, 22),
                Location = "101A"
            };

            // Act
            var result = ValidateAll(patientData, 1);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsEmpty(result.Errors);
        }

        [TestMethod]
        public void Validate_WithMissingName_ReturnsError()
        {
            // Arrange
            var patientData = new ExcelProviderAssignmentRow
            {
                Name = "",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 18,
                Admit = DateTime.Now
            };

            // Act
            var result = ValidateAll(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExcelProviderAssignmentRow.Name)));
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("required")));
        }

        [TestMethod]
        public void Validate_WithInvalidAge_ReturnsError()
        {
            // Arrange
            var patientData = new ExcelProviderAssignmentRow
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 200, // Invalid
                Admit = DateTime.Now
            };

            // Act
            var result = ValidateAll(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExcelProviderAssignmentRow.Age)));
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("0 and 150")));
        }

        [TestMethod]
        public void Validate_WithNegativeAge_ReturnsError()
        {
            // Arrange
            var patientData = new ExcelProviderAssignmentRow
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = -5,
                Admit = DateTime.Now
            };

            // Act
            var result = ValidateAll(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExcelProviderAssignmentRow.Age)));
        }

        [TestMethod]
        public void Validate_WithMissingHospitalNumber_ReturnsError()
        {
            // Arrange
            var patientData = new ExcelProviderAssignmentRow
            {
                Name = "Test Patient",
                HospitalNumber = "",
                Mrn = "127116",
                Age = 18,
                Admit = DateTime.Now
            };

            // Act
            var result = ValidateAll(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExcelProviderAssignmentRow.HospitalNumber)));
        }

        [TestMethod]
        public void Validate_WithFutureAdmitDate_ReturnsError()
        {
            // Arrange
            var patientData = new ExcelProviderAssignmentRow
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 18,
                Admit = DateTime.UtcNow.AddDays(1) // Future date
            };

            // Act
            var result = ValidateAll(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExcelProviderAssignmentRow.Admit)));
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("future")));
        }

        [TestMethod]
        public void Validate_WithFutureDob_ReturnsError()
        {
            // Arrange
            var patientData = new ExcelProviderAssignmentRow
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 18,
                Admit = DateTime.Now,
                Dob = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1) // Future date
            };

            // Act
            var result = ValidateAll(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExcelProviderAssignmentRow.Dob)));
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("future")));
        }

        [TestMethod]
        public void Validate_WithMissingMrn_ReturnsError()
        {
            // Arrange
            var patientData = new ExcelProviderAssignmentRow
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "",
                Age = 18,
                Admit = DateTime.Now
            };

            // Act
            var result = ValidateAll(patientData, 1);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.PropertyName == nameof(ExcelProviderAssignmentRow.Mrn)));
        }

        [TestMethod]
        public void Validate_WithMultipleErrors_ReturnsAllErrors()
        {
            // Arrange - raw Excel row with multiple validation errors
            var patientData = new ExcelProviderAssignmentRow
            {
                Name = "",
                HospitalNumber = "",
                Mrn = "",
                Age = -1,
                Admit = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var result = ValidateAll(patientData, 1);

            // Assert - expect 5 errors (Name, HospitalNumber, MRN, Age, Admit date)
            Assert.IsFalse(result.IsValid);
            Assert.IsGreaterThanOrEqualTo(5, result.Errors.Count, $"Expected at least 5 errors, got {result.Errors.Count}");
        }

        [TestMethod]
        public void Validate_WithValidNullDob_ReturnsSuccess()
        {
            // Arrange
            var patientData = new ExcelProviderAssignmentRow
            {
                Name = "Test Patient",
                HospitalNumber = "2504322",
                Mrn = "127116",
                Age = 18,
                Admit = DateTime.Now,
                Dob = null
            };

            // Act
            var result = ValidateAll(patientData, 1);

            // Assert
            Assert.IsTrue(result.IsValid);
        }
    }
}
