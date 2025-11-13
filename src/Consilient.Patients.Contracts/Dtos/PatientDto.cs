namespace Consilient.Patients.Contracts.Dtos
{
    public sealed record PatientDto
    {
        public int Id { get; set; }
        public int Mrn { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public DateOnly? BirthDate { get; init; }
    }
}
