using Consilient.Common;

namespace Consilient.Data.Entities
{
    public class Employee : BaseEntity<int>
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? TitleExtension { get; set; }

        public EmployeeRole Role { get; set; }

        public string Email { get; set; } = string.Empty;
    }
}