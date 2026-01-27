using Microsoft.AspNetCore.Http;

namespace Consilient.ProviderAssignments.Contracts;

/// <summary>
/// Request to import provider assignments from an uploaded file.
/// </summary>
public class ImportProviderAssignmentRequest
{
    /// <summary>
    /// The uploaded file containing provider assignments.
    /// </summary>
    public required IFormFile File { get; init; }

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
