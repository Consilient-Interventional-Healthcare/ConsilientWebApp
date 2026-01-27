namespace Consilient.ProviderAssignments.Contracts.Resolution;

/// <summary>
/// Progress information for the resolution phase.
/// </summary>
public class ResolutionProgressEventArgs : EventArgs
{
    /// <summary>
    /// Current stage of resolution (e.g., "Physician", "Patient", "SaveChanges").
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
}
