namespace Consilient.Data.Entities
{


    public class Visit : BaseEntity<int>
    {
        public virtual Employee? CosigningPhysicianEmployee { get; set; }
        public int? CosigningPhysicianEmployeeId { get; set; }
        public DateOnly DateServiced { get; set; }
        public virtual Hospitalization Hospitalization { get; set; } = null!;
        public int HospitalizationId { get; set; }

        public virtual Insurance? Insurance { get; set; }
        public int? InsuranceId { get; set; }

        public bool IsScribeServiceOnly { get; set; }
        public virtual Employee? NursePractitionerEmployee { get; set; }

        public int? NursePractitionerEmployeeId { get; set; }
        public virtual Employee PhysicianEmployee { get; set; } = null!;
        public int PhysicianEmployeeId { get; set; }
        public virtual Employee? ScribeEmployee { get; set; }

        public int? ScribeEmployeeId { get; set; }
        public virtual ServiceType ServiceType { get; set; } = null!;
        public int ServiceTypeId { get; set; }
        public virtual Patient Patient { get { return Hospitalization.Patient; } }
        public virtual Facility Facility { get { return Hospitalization.Facility; } }
    }
}