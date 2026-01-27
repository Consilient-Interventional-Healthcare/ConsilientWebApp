namespace Consilient.ProviderAssignments.Contracts;

/// <summary>
/// Result returned after uploading a provider assignment Excel file.
/// Contains the batch ID for tracking through the import pipeline.
/// </summary>
public class ImportProviderAssignmentResult
{
        /// <summary>
        /// Indicates whether the import operation was successful.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Status message describing the upload result.
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Unique batch identifier for tracking this import through resolution and processing.
    /// </summary>
    public Guid BatchId { get; set; }
}
