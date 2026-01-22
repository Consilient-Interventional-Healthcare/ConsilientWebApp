namespace Consilient.Data.Entities
{
    public class ProviderAssignmentBatch : BaseEntity<Guid>
    {
        public DateOnly Date { get; set; }

        public int FacilityId { get; set; }

        public ProviderAssignmentBatchStatus Status { get; set; }

        public virtual ICollection<ProviderAssignment> ProviderAssignments { get; set; } = [];
    }
}
