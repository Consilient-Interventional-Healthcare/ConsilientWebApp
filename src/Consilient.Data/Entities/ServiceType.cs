namespace Consilient.Data.Entities
{

    public class ServiceType : BaseEntity<int>
    {
        public string Description { get; set; } = string.Empty;

        public int? Cptcode { get; set; }
    }
}