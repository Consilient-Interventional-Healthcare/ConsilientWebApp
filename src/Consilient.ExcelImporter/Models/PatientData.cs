namespace Consilient.ExcelImporter.Models
{
    /// <summary>
    /// Represents the data structure for a patient record imported from an Excel file.
    /// </summary>
    public class PatientData
    {
        /// <summary>
        /// Gets or sets the Case ID.
        /// </summary>
        public string CaseID { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the patient's name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the Medical Record Number.
        /// </summary>
        public string MRN { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the patient's sex.
        /// </summary>
        public string Sex { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the patient's age.
        /// </summary>
        public int Age { get; set; }
        /// <summary>
        /// Gets or sets the patient's Date of Birth. This can be nullable.
        /// </summary>
        public DateTime? DOB { get; set; }
        /// <summary>
        /// Gets or sets the patient's room number.
        /// </summary>
        public string Room { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the patient's bed identifier.
        /// </summary>
        public string Bed { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the Date of Admission.
        /// </summary>
        public DateTime DOA { get; set; }
        /// <summary>
        /// Gets or sets the Length of Stay in days.
        /// </summary>
        public int LOS { get; set; }
        /// <summary>
        /// Gets or sets the attending physician's name.
        /// </summary>
        public string AttendingPhysician { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the primary insurance provider.
        /// </summary>
        public string PrimaryInsurance { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the admitting diagnosis.
        /// </summary>
        public string AdmDx { get; set; } = string.Empty;
    }
}