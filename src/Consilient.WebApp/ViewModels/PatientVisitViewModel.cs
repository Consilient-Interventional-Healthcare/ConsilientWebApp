using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Consilient.WebApp.ViewModels
{
    public class PatientVisitViewModel
    {
        public int PatientVisitId { get; set; }

        [Display(Name = "Date Serviced")]
        public DateOnly DateServiced { get; set; }

        //public int PatientId { get; set; }

        //public int FacilityId { get; set; }

        [Display(Name = "Case ID")]
        public int? AdmissionNumber { get; set; }

        //public int? InsuranceId { get; set; }

        //public int ServiceTypeId { get; set; }

        //public int PhysicianEmployeeId { get; set; }

        //public int? NursePractitionerEmployeeId { get; set; }

        //public int IsSupervising { get; set; }

        //public int? ScribeEmployeeId { get; set; }

        //public int? CosigningPhysicianEmployeeId { get; set; }

        [Display(Name = "Is Scribe Service Only")]
        public bool IsScribeServiceOnly { get; set; }

        [ValidateNever]
        [Display(Name = "Cosigning Physician")]
        public EmployeeViewModel? CosigningPhysicianEmployee { get; set; } = null!;

        [ValidateNever]
        [Display(Name = "Facility")]
        public FacilityViewModel? Facility { get; set; } = null!;

        [ValidateNever]
        [Display(Name = "Insurance")]
        public InsuranceViewModel? Insurance { get; set; } = null!;

        [ValidateNever]
        [Display(Name = "Nurse Practitioner")]
        public EmployeeViewModel? NursePractitionerEmployee { get; set; } = null!;

        [ValidateNever]
        public PatientViewModel? Patient { get; set; } = null!;

        [ValidateNever]
        [Display(Name = "Physician")]
        public EmployeeViewModel? PhysicianEmployee { get; set; } = null!;

        [ValidateNever]
        [Display(Name = "Scribe")]
        public EmployeeViewModel ScribeEmployee { get; set; } = null!;

        [ValidateNever]
        [Display(Name = "Service Type")]
        public ServiceTypeViewModel? ServiceType { get; set; } = null!;


        //public List<SelectListItem> CosigningPhysiciansSelectList { get; set; } = [];
        //public List<SelectListItem> PhysiciansSelectList { get; set; } = [];
        //public List<SelectListItem> NursePractitionersSelectList { get; set; } = [];
        //public List<SelectListItem> PatientSelectList { get; set; } = [];
        //public List<SelectListItem> InsurancesSelectList { get; set; } = [];
        //public List<SelectListItem> FacilitiesSelectList { get; set; } = [];
        //public List<SelectListItem> ServiceTypesSelectList { get; set; } = [];
    }
}
