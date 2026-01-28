namespace Consilient.Data.Entities.Clinical;

public class VisitEventTypeEntity : BaseEntity<int>
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public virtual ICollection<VisitEvent> VisitEvents { get; set; } = null!;
}
