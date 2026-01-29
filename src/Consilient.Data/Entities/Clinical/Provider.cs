using System.ComponentModel.DataAnnotations.Schema;

namespace Consilient.Data.Entities.Clinical;

public class Provider : BaseEntity<int>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? TitleExtension { get; set; }

    public int ProviderTypeId { get; set; }

    [NotMapped]
    public ProviderType Type
    {
        get => (ProviderType)ProviderTypeId;
        set => ProviderTypeId = (int)value;
    }

    public virtual ProviderTypeEntity? ProviderTypeNavigation { get; set; }

    public string Email { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }
}