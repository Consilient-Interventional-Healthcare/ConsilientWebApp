namespace Consilient.Shared.Contracts.Dtos;

/// <summary>
/// Data transfer object for service type.
/// </summary>
public class ServiceTypeDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    /// <summary>
    /// Valid billing codes for this service type.
    /// </summary>
    public List<BillingCodeAssociationDto> BillingCodes { get; set; } = [];
}