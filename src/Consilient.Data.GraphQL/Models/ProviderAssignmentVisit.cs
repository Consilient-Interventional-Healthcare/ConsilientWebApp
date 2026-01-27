namespace Consilient.Data.GraphQL.Models;


/// <summary>
/// Visit data for provider assignment - fetched from Visit table if resolved, otherwise from staging data
/// </summary>
public class ProviderAssignmentVisit
{
    public string? Room { get; set; }
    public string? Bed { get; set; }
}
