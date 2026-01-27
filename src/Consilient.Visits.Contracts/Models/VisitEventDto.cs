namespace Consilient.Visits.Contracts.Models;

public class VisitEventDto
{
    public string Description { get; set; } = string.Empty;
    public int EnteredByUserId { get; set; }
    public DateTime EventOccurredAt { get; set; }
    public int EventTypeId { get; set; }
    public int Id { get; set; }
    public int VisitId { get; set; }
}