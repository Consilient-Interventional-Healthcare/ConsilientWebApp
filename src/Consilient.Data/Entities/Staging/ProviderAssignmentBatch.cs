using System.ComponentModel.DataAnnotations.Schema;

namespace Consilient.Data.Entities.Staging;

public class ProviderAssignmentBatch : BaseEntity<Guid>
{
        public DateOnly Date { get; set; }

        public int FacilityId { get; set; }

        public int StatusId { get; set; }

        [NotMapped]
        public ProviderAssignmentBatchStatus Status
        {
            get => (ProviderAssignmentBatchStatus)StatusId;
            set => StatusId = (int)value;
        }

        public virtual ProviderAssignmentBatchStatusEntity? StatusNavigation { get; set; }

        public int CreatedByUserId { get; set; }

    public virtual ICollection<ProviderAssignment> ProviderAssignments { get; set; } = [];
}
