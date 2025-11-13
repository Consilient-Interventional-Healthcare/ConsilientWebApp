namespace Consilient.Data.Entities
{

    public class Insurance : BaseEntity<int>
    {
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool? PhysicianIncluded { get; set; }

        public bool? IsContracted { get; set; }
    }
}