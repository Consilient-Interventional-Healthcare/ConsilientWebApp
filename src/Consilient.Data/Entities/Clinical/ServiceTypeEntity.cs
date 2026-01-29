namespace Consilient.Data.Entities.Clinical;

public class ServiceTypeEntity : BaseEntity<int>
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}
