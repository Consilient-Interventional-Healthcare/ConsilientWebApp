namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers
{
    internal record PatientRow
    {
        public int PatientId { get; init; }
        public string PatientLastName { get; init; } = string.Empty;
        public string PatientFirstName { get; init; } = string.Empty;
        public string? PatientMrn { get; init; }
        public int? FacilityId { get; init; }
        public DateOnly? PatientDob { get; init; }
    }
}
