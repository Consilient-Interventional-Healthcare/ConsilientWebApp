namespace Consilient.Data.Entities
{
    public class HospitalizationStatusHistory : BaseEntity<int>
    {
        public int HospitalizationId { get; set; }
        public int NewStatusId { get; set; }
        public DateTime ChangedAt { get; set; }
        public int? ChangedByUserId { get; set; }
    }
}