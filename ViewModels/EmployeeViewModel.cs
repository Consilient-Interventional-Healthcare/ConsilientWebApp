using ConsilientWebApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ConsilientWebApp.ViewModels
{
    public class EmployeeViewModel
    {
        public int EmployeeId { get; set; }

        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Title Extension")]
        public string? TitleExtension { get; set; }

        [Display(Name = "Is a Provider")]
        public bool IsProvider { get; set; }

        [Display(Name = "Role")]
        public string? Role { get; set; }

        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Display(Name = "Is an Admin")] 
        public bool IsAdministrator { get; set; }

        public string? Email { get; set; }

        [ValidateNever]
        [Display(Name = "Contracts")]
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

        [ValidateNever]
        [Display(Name = "Patient Visit Cosigning Physician Employees")]
        public virtual ICollection<PatientVisitViewModel> PatientVisitCosigningPhysicianEmployees { get; set; } = new List<PatientVisitViewModel>();

        [ValidateNever]
        [Display(Name = "Patient Visit Nurse Practitioner Employees")]
        public virtual ICollection<PatientVisitViewModel> PatientVisitNursePractitionerEmployees { get; set; } = new List<PatientVisitViewModel>();

        [ValidateNever]
        [Display(Name = "Patient Visit Physician Employees")]
        public virtual ICollection<PatientVisitViewModel> PatientVisitPhysicianEmployees { get; set; } = new List<PatientVisitViewModel>();

        [ValidateNever]
        [Display(Name = "Patient Visit Scribe Employees")]
        public virtual ICollection<PatientVisitViewModel> PatientVisitScribeEmployees { get; set; } = new List<PatientVisitViewModel>();

        [ValidateNever]
        [Display(Name = "Patient Visits Staging Cosigning Physician Employees")]
        public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagingCosigningPhysicianEmployees { get; set; } = new List<PatientVisitsStagingViewModel>();

        [ValidateNever]
        [Display(Name = "Patient Visits Staging Nurse Practitioner Employees")]
        public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagingNursePractitionerEmployees { get; set; } = new List<PatientVisitsStagingViewModel>();

        [ValidateNever]
        [Display(Name = "Patient Visits Staging Physician Employees")]
        public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagingPhysicianEmployees { get; set; } = new List<PatientVisitsStagingViewModel>();

        [ValidateNever]
        [Display(Name = "Patient Visits Staging Scribe Employees")]
        public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagingScribeEmployees { get; set; } = new List<PatientVisitsStagingViewModel>();

        [ValidateNever]
        [Display(Name = "Provider Contracts")]
        public virtual ICollection<ProviderContract> ProviderContracts { get; set; } = new List<ProviderContract>();

        [ValidateNever]
        [Display(Name = "Provider Pay")]
        public virtual ICollection<ProviderPayViewModel> ProviderPays { get; set; } = new List<ProviderPayViewModel>();


        public SelectList RolesSelectList { get; set; } = new SelectList(new List<SelectListItem>
        {
            new SelectListItem { Value = "Physician", Text = "Physician" },
            new SelectListItem { Value = "Nurse Practitioner", Text = "Nurse Practitioner" },
            new SelectListItem { Value = "Scribe", Text = "Scribe" },
            new SelectListItem { Value = "Admin", Text = "Admin" }
        }, "Value", "Text");
    }
}
