namespace Consilient.Data.Entities
{
    public class Employee : BaseEntity<int>
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? TitleExtension { get; set; }

        public bool IsProvider { get; set; }

        public string? Role { get; set; }

        public bool IsAdministrator { get; set; }

        public string? Email { get; set; }

        public bool CanApproveVisits { get; set; }

        public virtual ICollection<VisitEvent> EnteredVisitEvents { get; set; } = null!;
    }
}