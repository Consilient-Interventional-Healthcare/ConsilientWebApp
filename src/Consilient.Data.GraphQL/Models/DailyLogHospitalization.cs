namespace Consilient.Data.GraphQL.Models;

public class DailyLogHospitalization
{
    public int Id { get; set; }
    public int HospitalizationStatusId { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public long CaseId { get; set; }
}
