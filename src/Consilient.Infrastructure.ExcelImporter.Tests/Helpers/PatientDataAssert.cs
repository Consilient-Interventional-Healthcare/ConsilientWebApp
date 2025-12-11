using Consilient.Infrastructure.ExcelImporter.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Helpers
{
    /// <summary>
    /// Assertion helpers for PatientData used by unit tests.
    /// </summary>
    public static class PatientDataAssert
    {
        /// <summary>
        /// Asserts that two <see cref="DoctorAssignment"/> instances have equal property values.
        /// Provides contextual message when <paramref name="context"/> is supplied.
        /// </summary>
        public static void AreEqual(DoctorAssignment expected, DoctorAssignment actual, string? context = null)
        {
            string MakeMsg(string msg) => context is null ? msg : $"{context}: {msg}";

            Assert.IsNotNull(expected, MakeMsg("expected PatientData is null"));
            Assert.IsNotNull(actual, MakeMsg("actual PatientData is null"));

            // String properties
            Assert.AreEqual(expected.Name, actual.Name, MakeMsg("Name mismatch"));
            Assert.AreEqual(expected.HospitalNumber, actual.HospitalNumber, MakeMsg("HospitalNumber mismatch"));
            Assert.AreEqual(expected.Mrn, actual.Mrn, MakeMsg("Mrn mismatch"));
            Assert.AreEqual(expected.Location, actual.Location, MakeMsg("Location mismatch"));
            Assert.AreEqual(expected.AttendingMD, actual.AttendingMD, MakeMsg("AttendingMD mismatch"));
            Assert.AreEqual(expected.Insurance, actual.Insurance, MakeMsg("Insurance mismatch"));
            Assert.AreEqual(expected.IsCleared, actual.IsCleared, MakeMsg("IsCleared mismatch"));
            Assert.AreEqual(expected.NursePractitioner, actual.NursePractitioner, MakeMsg("NursePractitioner mismatch"));
            Assert.AreEqual(expected.H_P, actual.H_P, MakeMsg("H_P mismatch"));
            Assert.AreEqual(expected.PsychEval, actual.PsychEval, MakeMsg("PsychEval mismatch"));

            // Numeric / value type properties
            Assert.AreEqual(expected.Age, actual.Age, MakeMsg("Age mismatch"));
            Assert.AreEqual(expected.FacilityId, actual.FacilityId, MakeMsg("FacilityId mismatch"));

            // Date/time properties - exact match expected by tests; include explicit message
            Assert.AreEqual(expected.Dob, actual.Dob, MakeMsg("Dob mismatch"));
            Assert.AreEqual(expected.Admit, actual.Admit, MakeMsg("Admit mismatch"));
            Assert.AreEqual(expected.ServiceDate, actual.ServiceDate, MakeMsg("ServiceDate mismatch"));
        }
    }
}