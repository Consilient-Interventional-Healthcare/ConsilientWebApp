namespace Consilient.Visits.Contracts.Requests
{
    public class UpdateVisitRequest
    {
        public int ServiceTypeId { get; set; }
        public bool IsScribeServiceOnly { get; set; }
        public bool PhysicianApproved { get; set; }
        public bool NursePractitionerApproved { get; set; }
        public DateTime PhysicianApprovedDateTime { get; set; }
        public string? PhysicianApprovedBy { get; set; }
    }
}