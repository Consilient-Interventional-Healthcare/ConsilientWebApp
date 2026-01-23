namespace Consilient.Data.GraphQL.Models
{
    /// <summary>
    /// Hospitalization data for provider assignment - fetched from Hospitalization table if resolved, otherwise from staging data
    /// </summary>
    public class ProviderAssignmentHospitalization
    {
        public string CaseId { get; set; } = string.Empty;
        public DateTime? AdmissionDate { get; set; }
    }
}
