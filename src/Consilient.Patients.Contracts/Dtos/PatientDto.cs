namespace Consilient.Patients.Contracts.Dtos
{
    public sealed record PatientDto
    {
        public int PatientId { get; init; }
        public int PatientMrn { get; init; }
        public string? PatientFirstName { get; init; }
        public string? PatientLastName { get; init; }
        public DateOnly? PatientBirthDate { get; init; }
        public string? PatientFullName { get; init; }
    }
}
