namespace Consilient.Visits.Contracts.Models.Requests;

public class InsertVisitEventRequest
{
    public int VisitId { get; set; }
    public int EventTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
}