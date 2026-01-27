namespace Consilient.Shared.Contracts.Dtos;

/// <summary>
/// Data transfer object for service type.
/// </summary>
public class ServiceTypeDto
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public int? CptCode { get; set; }
}