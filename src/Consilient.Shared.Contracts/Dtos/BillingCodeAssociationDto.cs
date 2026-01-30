namespace Consilient.Shared.Contracts.Dtos;

/// <summary>
/// Represents a billing code associated with a service type.
/// </summary>
public class BillingCodeAssociationDto
{
    /// <summary>
    /// The billing code (e.g., "99232").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// True if this is the default billing code for the service type.
    /// </summary>
    public bool IsDefault { get; set; }
}
