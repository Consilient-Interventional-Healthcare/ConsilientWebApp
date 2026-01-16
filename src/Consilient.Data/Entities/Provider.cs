using Consilient.Common;

namespace Consilient.Data.Entities
{
    public class Provider : BaseEntity<int>
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? TitleExtension { get; set; }

        public ProviderType Type { get; set; }

        public string Email { get; set; } = string.Empty;

        public int? EmployeeId { get; set; }
    }
}