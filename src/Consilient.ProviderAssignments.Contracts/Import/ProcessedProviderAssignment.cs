namespace Consilient.ProviderAssignments.Contracts.Import;

/// <summary>
/// Enriched provider assignment with import context and derived fields.
/// Produced by the transformation pipeline from ExcelProviderAssignmentRow.
/// </summary>
public record ProcessedProviderAssignment
{
    // Import context (injected, not from Excel)
    public required int FacilityId { get; init; }
    public required DateOnly ServiceDate { get; init; }

    // Composition: Raw data from Excel
    public required ExcelProviderAssignmentRow Raw { get; init; }

    // Derived/parsed fields (populated by transformers)
    public string? Room { get; init; }
    public string? Bed { get; init; }
    public string? NormalizedPatientLastName { get; init; }
    public string? NormalizedPatientFirstName { get; init; }
    public string? NormalizedPhysicianLastName { get; init; }
    public string? NormalizedNursePractitionerLastName { get; init; }
}
