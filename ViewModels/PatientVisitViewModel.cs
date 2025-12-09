using ConsilientWebApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ConsilientWebApp.ViewModels
{
    public class PatientVisitViewModel
    {
        public int PatientVisitId { get; set; }

        [Display(Name = "Date Serviced")]
        public DateOnly DateServiced { get; set; }

        public int PatientId { get; set; }

        public int FacilityId { get; set; }

        [Display(Name ="Case ID")]
        public int? AdmissionNumber { get; set; }

        public int ServiceTypeId { get; set; }

        public int PhysicianEmployeeId { get; set; }

        public int? NursePractitionerEmployeeId { get; set; }

        public int IsSupervising { get; set; }

        public int? ScribeEmployeeId { get; set; }

        public int? CosigningPhysicianEmployeeId { get; set; }

        [Display(Name = "Is Scribe Service Only")]
        public bool IsScribeServiceOnly { get; set; }

        [ValidateNever]
        [Display(Name = "Cosigning Physician")]
        public virtual EmployeeViewModel CosigningPhysicianEmployee { get; set; } = new EmployeeViewModel();

        [ValidateNever]
        [Display(Name = "Facility")]
        public virtual FacilityViewModel Facility { get; set; } = new FacilityViewModel();

        [ValidateNever]
        [Display(Name = "Nurse Practitioner")]
        public virtual EmployeeViewModel NursePractitionerEmployee { get; set; } = new EmployeeViewModel();

        [ValidateNever]
        public virtual PatientViewModel Patient { get; set; } = new PatientViewModel();

        [ValidateNever]
        [Display(Name = "Physician")]
        public virtual EmployeeViewModel PhysicianEmployee { get; set; } = new EmployeeViewModel();

        [ValidateNever]
        [Display(Name = "Scribe")]
        public virtual EmployeeViewModel ScribeEmployee { get; set; } = new EmployeeViewModel();

        [ValidateNever]
        [Display(Name = "Service Type")]
        public virtual ServiceTypeViewModel ServiceType { get; set; } = new ServiceTypeViewModel();


        public List<SelectListItem> CosigningPhysiciansSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PhysiciansSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> NursePractitionersSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PatientSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> FacilitiesSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ServiceTypesSelectList { get; set; } = new List<SelectListItem>();
    }
}
