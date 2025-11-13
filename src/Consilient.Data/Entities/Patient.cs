namespace Consilient.Data.Entities
{

    public class Patient : BaseEntity<int>
    {
        public int Mrn { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateOnly? BirthDate { get; set; }

    }
}