namespace Consilient.Data.Entities.Clinical;

public class VisitEventType : BaseEntity<int>
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public virtual ICollection<VisitEvent> VisitEvents { get; set; } = null!;
}
