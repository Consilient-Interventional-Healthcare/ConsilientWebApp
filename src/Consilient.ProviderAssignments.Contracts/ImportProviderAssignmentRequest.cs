namespace Consilient.ProviderAssignments.Contracts;

/// <summary>
/// Request to import provider assignments from an uploaded file.
/// </summary>
public class ImportProviderAssignmentRequest
{
    /// <summary>
    /// Stream containing the file data.
    /// </summary>
    public required Stream FileStream { get; init; }

    /// <summary>
    /// Original filename of the uploaded file.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// ID of the facility for these assignments.
    /// </summary>
    public required int FacilityId { get; init; }

    /// <summary>
    /// Date of service for the assignments.
    /// </summary>
    public required DateOnly ServiceDate { get; init; }

    /// <summary>
    /// ID of the user who initiated the import.
    /// </summary>
    public required int EnqueuedByUserId { get; init; }
}
