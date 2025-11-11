namespace Consilient.Data.Entities
{

    public class Patient : BaseEntity<int>
    {
        public int PatientMrn { get; set; }

        public string? PatientFirstName { get; set; }

        public string? PatientLastName { get; set; }

        public DateOnly? PatientBirthDate { get; set; }

    }
}