namespace Consilient.ProviderAssignments.Contracts.Import;

/// <summary>
/// Raw data record representing a single row from an Excel provider assignment file.
/// Contains only properties that map directly to Excel columns.
/// </summary>
public record ExcelProviderAssignmentRow
{
    public string Name { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string HospitalNumber { get; init; } = string.Empty;
    public DateTime Admit { get; init; }
    public string Mrn { get; init; } = string.Empty;
    public int Age { get; init; }
    public DateOnly? Dob { get; init; }
    public string Insurance { get; init; } = string.Empty;
    public string NursePractitioner { get; init; } = string.Empty;
    public string IsCleared { get; init; } = string.Empty;
    public string H_P { get; init; } = string.Empty;
    public string PsychEval { get; init; } = string.Empty;
    public string AttendingMD { get; init; } = string.Empty;
}
