namespace Consilient.Employees.Contracts.Requests
{
    public class UpdateEmployeeRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? TitleExtension { get; set; }
        public bool IsProvider { get; set; }
        public string? Role { get; set; }
        public bool IsAdministrator { get; set; }
        public bool CanApproveVisits { get; set; }
    }
}
