using System.ComponentModel.DataAnnotations;

namespace Consilient.WebApp.ViewModels
{
    public class InsuranceViewModel
    {
        public int InsuranceId { get; set; }

        [Display(Name = "Insurance Code")]
        public string? InsuranceCode { get; set; }

        [Display(Name = "Insurance Description")]
        public string? InsuranceDescription { get; set; }

        [Display(Name = "Physician Included")]
        public bool? PhysicianIncluded { get; set; }

        [Display(Name = "Is Contracted")]
        public bool? IsContracted { get; set; }

        [Display(Name = "Code and Description")]
        public string CodeAndDescription { get; set; } = null!;

        //[ValidateNever]
        //public virtual ICollection<PatientVisitViewModel> PatientVisits { get; set; } = [];
        //[ValidateNever]
        //public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagings { get; set; } = [];
    }
}
