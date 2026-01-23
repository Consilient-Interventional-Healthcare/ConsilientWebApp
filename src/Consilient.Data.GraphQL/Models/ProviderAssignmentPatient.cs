namespace Consilient.Data.GraphQL.Models
{
    /// <summary>
    /// Patient data for provider assignment - fetched from Patient table if resolved, otherwise from staging data
    /// </summary>
    public class ProviderAssignmentPatient
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Mrn { get; set; } = string.Empty;
    }
}
