using ConsilientWebApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ConsilientWebApp.ViewModels
{
    public class PatientVisitsStagingViewModel
    {
        public int PatientVisitStagingId { get; set; }

        [Display(Name = "Date Serviced")]
        public DateOnly DateServiced { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-1).Date);

        public int? PatientId { get; set; }

        public int FacilityId { get; set; }

        [Display(Name = "Case ID")]
        public int? AdmissionNumber { get; set; }

        public int? InsuranceId { get; set; }

        public int? ServiceTypeId { get; set; }

        public int PhysicianEmployeeId { get; set; }

        public int? NursePractitionerEmployeeId { get; set; }

        public int? ScribeEmployeeId { get; set; }

        public bool ScribeApproved { get; set; }

        [Display(Name = "Approved")]
        public bool QualityApproved { get; set; }

        public string? QualityApprovedBy { get; set; }

        public DateTime? QualityApprovedDateTime { get; set; }

        public bool AddedToMainTable { get; set; }

        public int? CosigningPhysicianEmployeeId { get; set; }


        [ValidateNever]
        [Display(Name = "Cosigning")]
        public virtual EmployeeViewModel CosigningPhysicianEmployee { get; set; } = new EmployeeViewModel();

        [ValidateNever]
        [Display(Name = "Facility")]
        public virtual FacilityViewModel Facility { get; set; } = new FacilityViewModel();

        [ValidateNever]
        [Display(Name = "Insurance")]
        public virtual InsuranceViewModel Insurance { get; set; } = new InsuranceViewModel();

        [ValidateNever]
        [Display(Name = "NP")]
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
        [Display(Name = "Service")]
        public virtual ServiceTypeViewModel ServiceType { get; set; } = new ServiceTypeViewModel();

        public List<SelectListItem> CosigningPhysiciansSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> InsurancesSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> FacilitiesSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> NursePractitionersSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PatientsSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PhysiciansSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ScribesSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ServiceTypesSelectList { get; set; } = new List<SelectListItem>();

        [ValidateNever]
        public virtual PatientViewModel NewPatient { get; set; } = new PatientViewModel();

    }
}
