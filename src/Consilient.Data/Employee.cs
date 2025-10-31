namespace Consilient.Data
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? TitleExtension { get; set; }

        public bool IsProvider { get; set; }

        public string? Role { get; set; }

        public string FullName { get; set; } = null!;

        public bool IsAdministrator { get; set; }

        public string? Email { get; set; }

        public bool CanApproveVisits { get; set; }

        //public virtual ICollection<Contract> Contracts { get; set; } = [];

        //public virtual ICollection<PatientVisit> PatientVisitCosigningPhysicianEmployees { get; set; } = [];

        //public virtual ICollection<PatientVisit> PatientVisitNursePractitionerEmployees { get; set; } = [];

        //public virtual ICollection<PatientVisit> PatientVisitPhysicianEmployees { get; set; } = [];

        //public virtual ICollection<PatientVisit> PatientVisitScribeEmployees { get; set; } = [];

        //public virtual ICollection<PatientVisitStaging> PatientVisitsStagingCosigningPhysicianEmployees { get; set; } =
        //    [];

        //public virtual ICollection<PatientVisitStaging> PatientVisitsStagingNursePractitionerEmployees { get; set; } =
        //    [];

        //public virtual ICollection<PatientVisitStaging> PatientVisitsStagingPhysicianEmployees { get; set; } = [];

        //public virtual ICollection<PatientVisitStaging> PatientVisitsStagingScribeEmployees { get; set; } = [];

        //public virtual ICollection<ProviderContract> ProviderContracts { get; set; } = [];

        //public virtual ICollection<ProviderPay> ProviderPays { get; set; } = [];
    }
}