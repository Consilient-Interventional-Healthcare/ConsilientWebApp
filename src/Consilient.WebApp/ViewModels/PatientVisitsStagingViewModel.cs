using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Consilient.WebApp.ViewModels
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

        [Display(Name = "NP Approved")]
        public bool NursePractitionerApproved { get; set; }

        [Display(Name = "Physician Approved")]
        public bool PhysicianApproved { get; set; }

        //[Display(Name = "Physician Approved By")]
        //public string? PhysicianApprovedBy { get; set; }

        //[Display(Name = "Physician Approved Date/Time")]
        //public DateTime? PhysicianApprovedDateTime { get; set; }

        public bool AddedToMainTable { get; set; }

        public int? CosigningPhysicianEmployeeId { get; set; }

        [Display(Name = "Is Scribe Service Only")]
        public bool IsScribeServiceOnly { get; set; }


        [ValidateNever]
        [Display(Name = "Cosigning")]
        public EmployeeViewModel CosigningPhysicianEmployee { get; set; } = null!;

        [ValidateNever]
        [Display(Name = "Facility")]
        public FacilityViewModel Facility { get; set; } = null!;

        //[ValidateNever]
        //[Display(Name = "Insurance")]
        //public InsuranceViewModel Insurance { get; set; } = new InsuranceViewModel();

        [ValidateNever]
        [Display(Name = "NP")]
        public EmployeeViewModel NursePractitionerEmployee { get; set; } = null!;

        [ValidateNever]
        public PatientViewModel Patient { get; set; } = null!;

        [ValidateNever]
        [Display(Name = "Physician")]
        public EmployeeViewModel PhysicianEmployee { get; set; } = null!;

        [ValidateNever]
        [Display(Name = "Scribe")]
        public EmployeeViewModel ScribeEmployee { get; set; } = null!;

        [ValidateNever]
        [Display(Name = "Service")]
        public ServiceTypeViewModel ServiceType { get; set; } = null!;

        public List<SelectListItem> CosigningPhysiciansSelectList { get; set; } = [];
        //public List<SelectListItem> InsurancesSelectList { get; set; } = [];
        public List<SelectListItem> FacilitiesSelectList { get; set; } = [];
        public List<SelectListItem> NursePractitionersSelectList { get; set; } = [];
        public List<SelectListItem> PatientsSelectList { get; set; } = [];
        public List<SelectListItem> PhysiciansSelectList { get; set; } = [];
        public List<SelectListItem> ScribesSelectList { get; set; } = [];
        public List<SelectListItem> ServiceTypesSelectList { get; set; } = [];

        [ValidateNever]
        public PatientViewModel NewPatient { get; set; } = null!;

    }
}
