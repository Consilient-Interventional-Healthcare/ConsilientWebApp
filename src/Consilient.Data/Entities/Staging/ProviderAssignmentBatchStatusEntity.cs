namespace Consilient.Data.Entities.Staging;

public class ProviderAssignmentBatchStatusEntity : BaseEntity<int>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
