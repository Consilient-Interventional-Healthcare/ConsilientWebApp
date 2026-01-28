namespace Consilient.Data.GraphQL.Models;

public class DailyLogLogEntryV2
{
    public DailyLogEvent Event { get; set; } = null!;
    public DailyLogUser User { get; set; } = null!;
    public DailyLogEventType? EventType { get; set; }
}

public class DailyLogEvent
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int EnteredByUserId { get; set; }
    public string EventOccurredAt { get; set; } = string.Empty;
    public int EventTypeId { get; set; }
    public int VisitId { get; set; }
}

public class DailyLogUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class DailyLogEventType
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
