namespace Consilient.Patients.Contracts.Requests
{
    public class CreatePatientVisitRequest
    {
        public DateOnly DateServiced { get; set; }

        public int PatientId { get; set; }

        public int FacilityId { get; set; }

        public int? AdmissionNumber { get; set; }

        public int? InsuranceId { get; set; }

        public int ServiceTypeId { get; set; }

        public int PhysicianEmployeeId { get; set; }

        public int? NursePractitionerEmployeeId { get; set; }

        public int IsSupervising { get; set; }

        public int? ScribeEmployeeId { get; set; }

        public int? CosigningPhysicianEmployeeId { get; set; }

        public bool IsScribeServiceOnly { get; set; }

    }
}