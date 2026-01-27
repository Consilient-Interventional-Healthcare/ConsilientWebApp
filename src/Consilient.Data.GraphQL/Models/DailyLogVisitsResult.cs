namespace Consilient.Data.GraphQL.Models;

public class DailyLogVisitsResult
{
    public DateOnly Date { get; set; }
    public int FacilityId { get; set; }
    public List<DailyLogProvider> Providers { get; set; } = [];
    public List<DailyLogVisit> Visits { get; set; } = [];
}
