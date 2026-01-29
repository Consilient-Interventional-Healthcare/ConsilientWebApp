using Consilient.Common;

namespace Consilient.Employees.Contracts.Requests;

public class UpdateEmployeeRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? TitleExtension { get; set; }
    public int Role { get; set; }
    public bool CanApproveVisits { get; set; }
}
