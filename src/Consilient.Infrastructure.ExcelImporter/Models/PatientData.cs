namespace Consilient.Infrastructure.ExcelImporter.Models
{
    /// <summary>
    /// Represents the data structure for a patient record imported from an Excel file.
    /// </summary>
    public class PatientData
    {
        /// <summary>
        /// Gets or sets the Case ID.
        /// </summary>
        public string CaseId { get; init; } = string.Empty;
        /// <summary>
        /// Gets or sets the patient's name.
        /// </summary>
        public string Name { get; init; } = string.Empty;
        /// <summary>
        /// Gets or sets the Medical Record Number.
        /// </summary>
        public string Mrn { get; init; } = string.Empty;
        /// <summary>
        /// Gets or sets the patient's sex.
        /// </summary>
        public string Sex { get; init; } = string.Empty;
        /// <summary>
        /// Gets or sets the patient's age.
        /// </summary>
        public int Age { get; init; }
        /// <summary>
        /// Gets or sets the patient's Date of Birth. This can be nullable.
        /// </summary>
        public DateTime? Dob { get; init; }
        /// <summary>
        /// Gets or sets the patient's room number.
        /// </summary>
        public string Room { get; init; } = string.Empty;
        /// <summary>
        /// Gets or sets the patient's bed identifier.
        /// </summary>
        public string Bed { get; init; } = string.Empty;
        /// <summary>
        /// Gets or sets the Date of Admission.
        /// </summary>
        public DateTime Doa { get; init; }
        /// <summary>
        /// Gets or sets the Length of Stay in days.
        /// </summary>
        public int Los { get; init; }
        /// <summary>
        /// Gets or sets the attending physician's name.
        /// </summary>
        public string AttendingPhysician { get; init; } = string.Empty;
        /// <summary>
        /// Gets or sets the primary insurance provider.
        /// </summary>
        public string PrimaryInsurance { get; init; } = string.Empty;
        /// <summary>
        /// Gets or sets the admitting diagnosis.
        /// </summary>
        public string AdmDx { get; init; } = string.Empty;
    }
}