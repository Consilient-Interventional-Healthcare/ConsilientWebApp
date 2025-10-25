namespace Consilient.Shared.Contracts.Requests
{
    /// <summary>
    /// Request to update an existing service type.
    /// Use null for properties you do not want to change.
    /// </summary>
    public class UpdateServiceTypeRequest
    {
        /// <summary>
        /// New description value, or null to leave unchanged.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// New CPT code value, or null to leave unchanged.
        /// </summary>
        public int? CptCode { get; set; }
    }
}