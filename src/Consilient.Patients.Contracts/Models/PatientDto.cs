using Consilient.Common;

namespace Consilient.Patients.Contracts.Models;

public sealed record PatientDto
{
    public int Id { get; set; }
    public string Mrn { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
}
