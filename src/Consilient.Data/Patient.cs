namespace Consilient.Data
{

    public class Patient
    {
        public int PatientId { get; set; }

        public int PatientMrn { get; set; }

        public string? PatientFirstName { get; set; }

        public string? PatientLastName { get; set; }

        public DateOnly? PatientBirthDate { get; set; }

        public string PatientFullName { get; set; } = null!;

        //public virtual ICollection<PatientVisit> PatientVisits { get; set; } = [];

        //public virtual ICollection<PatientVisitStaging> PatientVisitsStagings { get; set; } = [];
    }
}