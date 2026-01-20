namespace Consilient.ProviderAssignments.Contracts.Import
{
    /// <summary>
    /// Result returned after uploading a provider assignment Excel file.
    /// Contains the batch ID for tracking through the import pipeline.
    /// </summary>
    public class FileUploadResult
    {
        /// <summary>
        /// Original name of the uploaded file.
        /// </summary>
        public string FileName { get; set; } = null!;

        /// <summary>
        /// Service date for the imported assignments.
        /// </summary>
        public DateOnly ServiceDate { get; set; }

        /// <summary>
        /// Facility ID the assignments belong to.
        /// </summary>
        public int FacilityId { get; set; }

        /// <summary>
        /// Status message describing the upload result.
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Unique batch identifier for tracking this import through resolution and processing.
        /// </summary>
        public Guid BatchId { get; set; }
    }
}
