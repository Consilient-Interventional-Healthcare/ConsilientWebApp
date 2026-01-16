namespace Consilient.Employees.Contracts.Dtos
{
    public class EmployeeVisitCountDto
    {
        public int ProviderId { get; set; }
        public string ProviderLastName { get; set; } = string.Empty;
        public string ProviderFirstName { get; set; } = string.Empty;
        public int ProviderType { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbreviation { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string PatientMRN { get; set; } = string.Empty;
        public string PatientLastName { get; set; } = string.Empty;
        public string PatientFirstName { get; set; } = string.Empty;
        public int VisitId { get; set; }
        public DateOnly DateServiced { get; set; }
        public string Room { get; set; } = string.Empty;
        public string Bed { get; set; } = string.Empty;
    }
}
