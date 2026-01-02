using System.ComponentModel.DataAnnotations;

namespace Consilient.WebApp.ViewModels
{
    public class InsuranceViewModel
    {
        public int InsuranceId { get; init; }

        [Display(Name = "Insurance Code")]
        public string? InsuranceCode { get; init; }

        [Display(Name = "Insurance Description")]
        public string? InsuranceDescription { get; init; }

        [Display(Name = "Physician Included")]
        public bool? PhysicianIncluded { get; init; }

        [Display(Name = "Is Contracted")]
        public bool? IsContracted { get; init; }

        [Display(Name = "Code and Description")]
        public string CodeAndDescription { get; init; } = null!;

        //[ValidateNever]
        //public virtual ICollection<PatientVisitViewModel> PatientVisits { get; set; } = [];
        //[ValidateNever]
        //public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagings { get; set; } = [];
    }
}
