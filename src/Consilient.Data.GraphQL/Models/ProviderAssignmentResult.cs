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

    /// <summary>
    /// Provider data for provider assignment - fetched from Provider table if resolved, otherwise from staging data
    /// </summary>
    public class ProviderAssignmentProvider
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Hospitalization data for provider assignment - fetched from Hospitalization table if resolved, otherwise from staging data
    /// </summary>
    public class ProviderAssignmentHospitalization
    {
        public string CaseId { get; set; } = string.Empty;
        public DateTime? AdmissionDate { get; set; }
    }

    /// <summary>
    /// Visit data for provider assignment - fetched from Visit table if resolved, otherwise from staging data
    /// </summary>
    public class ProviderAssignmentVisit
    {
        public string? Room { get; set; }
        public string? Bed { get; set; }
        public bool Imported { get; set; }
    }
}
