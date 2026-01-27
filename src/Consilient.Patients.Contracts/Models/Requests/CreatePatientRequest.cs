namespace Consilient.Patients.Contracts.Models.Requests;

public class CreatePatientRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly? PatientBirthDate { get; init; }
    public int? FacilityId { get; init; }
    public string? Mrn { get; init; }
}
