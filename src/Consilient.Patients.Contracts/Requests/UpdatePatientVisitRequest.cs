namespace Consilient.Patients.Contracts.Requests
{
    public class UpdatePatientVisitRequest
    {

        public DateOnly DateServiced { get; set; }


        public int FacilityId { get; set; }

        public int? InsuranceId { get; set; }

        public int ServiceTypeId { get; set; }

        public int PhysicianEmployeeId { get; set; }

        public int? NursePractitionerEmployeeId { get; set; }

        public int? ScribeEmployeeId { get; set; }

        public int? CosigningPhysicianEmployeeId { get; set; }

        public bool IsScribeServiceOnly { get; set; }
        public bool PhysicianApproved { get; set; }
        public bool NursePractitionerApproved { get; set; }
        public DateTime PhysicianApprovedDateTime { get; set; }
        public string? PhysicianApprovedBy { get; set; }
    }
}