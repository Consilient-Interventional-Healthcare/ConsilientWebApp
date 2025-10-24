namespace Consilient.Shared.Contracts.Dtos
{
    /// <summary>
    /// Data transfer object for service type.
    /// </summary>
    public class ServiceTypeDto
    {
        public int ServiceTypeId { get; set; }

        public string? Description { get; set; }

        public int? CptCode { get; set; }

        /// <summary>
        /// Computed "CodeAndDescription" value from the database view, if available.
        /// </summary>
        public string? CodeAndDescription { get; set; }
    }
}