namespace Consilient.Insurances.Contracts.Dtos
{
    public class InsuranceDto
    {
        public int Id { get; set; }

        public string? InsuranceCode { get; set; }

        public string? InsuranceDescription { get; set; }

        public bool? PhysicianIncluded { get; set; }

        public bool? IsContracted { get; set; }

        public string CodeAndDescription { get; set; } = null!;
    }
}
