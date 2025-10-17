namespace Consilient.ExcelImporter.Constants
{
    /// <summary>
    /// Contains constant strings for expected Excel column headers.
    /// </summary>
    internal static class ExcelHeader
    {
        public const string CaseId = "Case ID";
        public const string Name = "Name";
        public const string Mrn = "MRN";
        public const string Sex = "Sex";
        public const string Age = "Age";
        public const string Dob = "DOB";
        public const string Room = "Room";
        public const string Bed = "Bed";
        public const string Doa = "DOA";
        public const string Los = "LOS";
        public const string AttendingPhysician = "Attending Physician";
        public const string PrimaryInsurance = "Primary Insurance";
        public const string AdmDx = "AdmDx";

        /// <summary>
        /// An array of the primary expected header names.
        /// </summary>
        public static readonly string[] ExpectedHeaders =
        [
            CaseId, Name, Mrn, Sex, Age, Dob, Room, Bed,
            Doa, Los, AttendingPhysician, PrimaryInsurance, AdmDx
        ];
    }
}