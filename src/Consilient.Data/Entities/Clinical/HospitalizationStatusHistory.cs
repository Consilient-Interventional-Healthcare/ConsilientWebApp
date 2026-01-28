using Consilient.Common;

namespace Consilient.Data.Entities.Clinical;

public class HospitalizationStatusHistory : BaseEntity<int>
{
    public int HospitalizationId { get; set; }

    public HospitalizationStatus NewStatus { get; set; }

    public virtual HospitalizationStatusEntity? NewStatusNavigation { get; set; }

    public DateTime ChangedAt { get; set; }
    public int? ChangedByUserId { get; set; }
}
