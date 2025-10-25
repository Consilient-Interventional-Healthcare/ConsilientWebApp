namespace Consilient.Shared.Contracts.Requests
{
    /// <summary>
    /// Request to create a new service type.
    /// </summary>
    public class CreateServiceTypeRequest
    {
        /// <summary>
        /// Human-readable description of the service type (e.g. "Office Visit").
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Optional CPT code value for the service type.
        /// </summary>
        public int? CptCode { get; set; }
    }
}