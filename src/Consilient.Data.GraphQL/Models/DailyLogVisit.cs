namespace Consilient.Data.GraphQL.Models;

public class DailyLogVisit
{
    public int Id { get; set; }
    public string Room { get; set; } = string.Empty;
    public string Bed { get; set; } = string.Empty;
    public DailyLogHospitalization Hospitalization { get; set; } = null!;
    public VisitPatient Patient { get; set; } = null!;
    public List<int> ProviderIds { get; set; } = [];
}
