using Consilient.Infrastructure.ExcelImporter.Transformers;
using Consilient.ProviderAssignments.Contracts.Import;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Unit;

[TestClass]
public class TransformerTests
{
    [TestMethod]
    public void TrimStringsTransformer_TrimsAllStringProperties()
    {
        // Arrange
        var transformer = new TrimStringsTransformer<ExcelProviderAssignmentRow>();
        var patientData = new ExcelProviderAssignmentRow
        {
            Name = "  Wymer, Mias  ",
            Location = "  101A  ",
            HospitalNumber = "  2504322  ",
            Mrn = "  127116  "
        };

        // Act
        var result = transformer.Transform(patientData);

        // Assert
        Assert.AreEqual("Wymer, Mias", result.Name);
        Assert.AreEqual("101A", result.Location);
        Assert.AreEqual("2504322", result.HospitalNumber);
        Assert.AreEqual("127116", result.Mrn);
    }

    [TestMethod]
    public void TrimStringsTransformer_HandlesEmptyStrings()
    {
        // Arrange
        var transformer = new TrimStringsTransformer<ExcelProviderAssignmentRow>();
        var patientData = new ExcelProviderAssignmentRow
        {
            Name = "",
            Location = "   ",
            HospitalNumber = "2504322"
        };

        // Act
        var result = transformer.Transform(patientData);

        // Assert
        Assert.AreEqual("", result.Name);
        Assert.AreEqual("", result.Location);
        Assert.AreEqual("2504322", result.HospitalNumber);
    }

    //[TestMethod]
    //public void CalculateAgeFromDobTransformer_CalculatesAgeWhenAgeIsZero()
    //{
    //    // Arrange
    //    var transformer = new CalculateAgeFromDobTransformer();
    //    var dob = DateTime.Today.AddYears(-25);
    //    var patientData = new DoctorAssignment
    //    {
    //        Name = "Test",
    //        Age = 0,
    //        Dob = dob
    //    };

    //    // Act
    //    var result = transformer.Transform(patientData);

    //    // Assert
    //    Assert.AreEqual(25, result.Age);
    //}

    //[TestMethod]
    //public void CalculateAgeFromDobTransformer_DoesNotOverwriteExistingAge()
    //{
    //    // Arrange
    //    var transformer = new CalculateAgeFromDobTransformer();
    //    var dob = DateTime.Today.AddYears(-25);
    //    var patientData = new DoctorAssignment
    //    {
    //        Name = "Test",
    //        Age = 30, // Already set
    //        Dob = dob
    //    };

    //    // Act
    //    var result = transformer.Transform(patientData);

    //    // Assert
    //    Assert.AreEqual(30, result.Age, "Age should not be overwritten when already set");
    //}

    //[TestMethod]
    //public void CalculateAgeFromDobTransformer_HandlesNullDob()
    //{
    //    // Arrange
    //    var transformer = new CalculateAgeFromDobTransformer();
    //    var patientData = new DoctorAssignment
    //    {
    //        Name = "Test",
    //        Age = 0,
    //        Dob = null
    //    };

    //    // Act
    //    var result = transformer.Transform(patientData);

    //    // Assert
    //    Assert.AreEqual(0, result.Age);
    //}

    //[TestMethod]
    //public void CalculateAgeFromDobTransformer_HandlesUpcomingBirthday()
    //{
    //    // Arrange
    //    var transformer = new CalculateAgeFromDobTransformer();
    //    // DOB is 25 years ago tomorrow (birthday hasn't happened yet this year)
    //    var dob = DateTime.Today.AddYears(-25).AddDays(1);
    //    var patientData = new DoctorAssignment
    //    {
    //        Name = "Test",
    //        Age = 0,
    //        Dob = dob
    //    };

    //    // Act
    //    var result = transformer.Transform(patientData);

    //    // Assert
    //    Assert.AreEqual(24, result.Age, "Should be 24 since birthday hasn't occurred yet");
    //}

    //[TestMethod]
    //public void CalculateAgeFromDobTransformer_HandlesBirthdayToday()
    //{
    //    // Arrange
    //    var transformer = new CalculateAgeFromDobTransformer();
    //    var dob = DateTime.Today.AddYears(-25);
    //    var patientData = new DoctorAssignment
    //    {
    //        Name = "Test",
    //        Age = 0,
    //        Dob = dob
    //    };

    //    // Act
    //    var result = transformer.Transform(patientData);

    //    // Assert
    //    Assert.AreEqual(25, result.Age, "Should be 25 on birthday");
    //}
}
