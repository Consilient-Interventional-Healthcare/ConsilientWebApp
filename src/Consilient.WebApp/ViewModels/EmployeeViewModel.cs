using Consilient.WebApp.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Consilient.WebApp.ViewModels
{
    public class EmployeeViewModel
    {
        public int EmployeeId { get; init; }

        [Display(Name = "First Name")]
        public string? FirstName { get; init; }

        [Display(Name = "Last Name")]
        public string? LastName { get; init; }

        [Display(Name = "Title Extension")]
        public string? TitleExtension { get; init; }

        [Display(Name = "Is a Provider")]
        public bool IsProvider { get; init; }

        [Display(Name = "Role")]
        public string? Role { get; init; }

        [Display(Name = "Full Name")]
        public string? FullName { get; init; }

        [Display(Name = "Is an Admin")]
        public bool IsAdministrator { get; init; }

        public string? Email { get; init; }

        [Display(Name = "Can Approve Visits")]
        public bool CanApproveVisits { get; init; }

        //[ValidateNever]
        //[Display(Name = "Contracts")]
        //public virtual ICollection<Contract> Contracts { get; set; } = [];

        //[ValidateNever]
        //[Display(Name = "Patient Visit Cosigning Physician Employees")]
        //public virtual ICollection<PatientVisitViewModel> PatientVisitCosigningPhysicianEmployees { get; set; } = [];

        //[ValidateNever]
        //[Display(Name = "Patient Visit Nurse Practitioner Employees")]
        //public virtual ICollection<PatientVisitViewModel> PatientVisitNursePractitionerEmployees { get; set; } = [];

        //[ValidateNever]
        //[Display(Name = "Patient Visit Physician Employees")]
        //public virtual ICollection<PatientVisitViewModel> PatientVisitPhysicianEmployees { get; set; } = [];

        //[ValidateNever]
        //[Display(Name = "Patient Visit Scribe Employees")]
        //public virtual ICollection<PatientVisitViewModel> PatientVisitScribeEmployees { get; set; } = [];

        //[ValidateNever]
        //[Display(Name = "Patient Visits Staging Cosigning Physician Employees")]
        //public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagingCosigningPhysicianEmployees { get; set; } = [];

        //[ValidateNever]
        //[Display(Name = "Patient Visits Staging Nurse Practitioner Employees")]
        //public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagingNursePractitionerEmployees { get; set; } = [];

        //[ValidateNever]
        //[Display(Name = "Patient Visits Staging Physician Employees")]
        //public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagingPhysicianEmployees { get; set; } = [];

        //[ValidateNever]
        //[Display(Name = "Patient Visits Staging Scribe Employees")]
        //public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagingScribeEmployees { get; set; } = [];

        //[ValidateNever]
        //[Display(Name = "Provider Contracts")]
        //public virtual ICollection<ProviderContract> ProviderContracts { get; set; } = [];

        //[ValidateNever]
        //[Display(Name = "Provider Pay")]
        //public virtual ICollection<ProviderPayViewModel> ProviderPays { get; set; } = [];


        public SelectList RolesSelectList { get; } = SelectListHelpers.GetRolesSelectList();
    }
}
