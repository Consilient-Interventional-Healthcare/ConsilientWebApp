using Consilient.Common;

namespace Consilient.Employees.Contracts.Requests;

public class UpdateEmployeeRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? TitleExtension { get; set; }
    //public bool IsProvider { get; set; }
    public ProviderType Role { get; set; }
    //public bool IsAdministrator { get; set; }
    public bool CanApproveVisits { get; set; }
}
