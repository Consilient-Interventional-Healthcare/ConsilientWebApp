using Consilient.Common;

namespace Consilient.Data.Entities
{
    public class Patient : BaseEntity<int>
    {
        public DateOnly? BirthDate { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public Gender? Gender { get; set; }
        public string LastName { get; set; } = string.Empty;
        public int Mrn { get; set; }
    }
}