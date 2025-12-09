namespace Consilient.Patients.Contracts.Models.Requests
{
    public class CreatePatientRequest
    {
        public int Mrn { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public DateOnly? PatientBirthDate { get; init; }
    }
}
