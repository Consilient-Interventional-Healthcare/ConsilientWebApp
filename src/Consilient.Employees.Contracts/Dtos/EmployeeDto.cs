namespace Consilient.Employees.Contracts.Dtos
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? TitleExtension { get; set; }
        public bool IsProvider { get; set; }
        public string? Role { get; set; }
        public string? FullName { get; set; }
        public bool IsAdministrator { get; set; }
        public string? Email { get; set; }
        public bool CanApproveVisits { get; set; }
    }
}
