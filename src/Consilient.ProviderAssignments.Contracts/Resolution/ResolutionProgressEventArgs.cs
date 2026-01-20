namespace Consilient.ProviderAssignments.Contracts.Resolution
{
    /// <summary>
    /// Progress information for the resolution phase.
    /// </summary>
    public class ResolutionProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Current stage of resolution (e.g., "Physician", "Patient", "BulkUpdate").
        /// </summary>
        public required string Stage { get; init; }

        /// <summary>
        /// Number of records processed so far.
        /// </summary>
        public int ProcessedRecords { get; init; }

        /// <summary>
        /// Total number of records to process.
        /// </summary>
        public int TotalRecords { get; init; }

        /// <summary>
        /// The batch ID being resolved.
        /// </summary>
        public Guid BatchId { get; init; }

        /// <summary>
        /// Current step number (1-based) out of total steps.
        /// </summary>
        public int CurrentStep { get; init; }

        /// <summary>
        /// Total number of steps in the resolution process.
        /// </summary>
        public int TotalSteps { get; init; }

        /// <summary>
        /// Progress percentage (0-100).
        /// </summary>
        public int PercentComplete => TotalSteps > 0 ? (CurrentStep * 100) / TotalSteps : 0;
    }
}
