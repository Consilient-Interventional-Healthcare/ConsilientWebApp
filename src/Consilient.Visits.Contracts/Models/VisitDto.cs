namespace Consilient.Visits.Contracts.Models
{
    public class VisitDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientLastName { get; set; } = string.Empty;
        public string PatientFirstName { get; set; } = string.Empty;
    }
}
