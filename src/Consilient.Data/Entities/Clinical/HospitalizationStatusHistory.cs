using System.ComponentModel.DataAnnotations.Schema;

namespace Consilient.Data.Entities.Clinical;

public class HospitalizationStatusHistory : BaseEntity<int>
{
    public int HospitalizationId { get; set; }

    public int NewStatusId { get; set; }

    [NotMapped]
    public HospitalizationStatus NewStatus
    {
        get => (HospitalizationStatus)NewStatusId;
        set => NewStatusId = (int)value;
    }

    public virtual HospitalizationStatusEntity? NewStatusNavigation { get; set; }

    public DateTime ChangedAt { get; set; }
    public int? ChangedByUserId { get; set; }
}
