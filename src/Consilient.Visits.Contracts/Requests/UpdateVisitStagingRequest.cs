namespace Consilient.Visits.Contracts.Requests
{
    public class UpdateVisitStagingRequest
    {
        public int? CosigningPhysicianEmployeeId { get; set; }
        public int FacilityId { get; set; }
        public int? InsuranceId { get; set; }
        public bool IsScribeServiceOnly { get; set; }
        public bool NursePractitionerApproved { get; set; }
        public int? NursePractitionerEmployeeId { get; set; }
        public bool PhysicianApproved { get; set; }
        public string? PhysicianApprovedBy { get; set; }
        public DateTime PhysicianApprovedDateTime { get; set; }
        public int PhysicianEmployeeId { get; set; }
        public int? ScribeEmployeeId { get; set; }
        public int ServiceTypeId { get; set; }
    }
}