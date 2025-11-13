namespace Consilient.Employees.Contracts.Dtos
{
    public class EmployeeVisitCountDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeLastName { get; set; } = string.Empty;
        public string EmployeeFirstName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int FacilityId { get; set; }
        public string FacilityAbbreviation { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public int PatientMRN { get; set; }
        public string PatientLastName { get; set; } = string.Empty;
        public string PatientFirstName { get; set; } = string.Empty;
        public int VisitId { get; set; }
        public DateOnly DateServiced { get; set; }
        public string Room { get; set; } = string.Empty;
        public string Bed { get; set; } = string.Empty;
    }
}
