namespace Consilient.Patients.Contracts.Requests
{
    public class CreatePatientRequest
    {
        public int PatientMrn { get; init; }
        public string? PatientFirstName { get; init; }
        public string? PatientLastName { get; init; }
        public DateOnly? PatientBirthDate { get; init; }
    }
}
