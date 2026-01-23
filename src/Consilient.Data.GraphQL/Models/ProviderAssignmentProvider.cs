namespace Consilient.Data.GraphQL.Models
{
    /// <summary>
    /// Provider data for provider assignment - fetched from Provider table if resolved, otherwise from staging data
    /// </summary>
    public class ProviderAssignmentProvider
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
