using System.ComponentModel.DataAnnotations;

namespace Consilient.WebApp.ViewModels
{
    public class FacilityViewModel
    {
        public int FacilityId { get; set; }

        [Display(Name = "Facility Name")]
        public string? FacilityName { get; set; }

        [Display(Name = "Facility Abbreviation")]
        public string? FacilityAbbreviation { get; set; }

        //[ValidateNever]
        //public virtual ICollection<ContractViewModel> Contracts { get; set; } = [];

        //[ValidateNever]
        //public virtual ICollection<FacilityPayViewModel> FacilityPays { get; set; } = [];

        //[ValidateNever]
        //public virtual ICollection<PatientVisitViewModel> PatientVisits { get; set; } = [];

        //[ValidateNever]
        //public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagings { get; set; } = [];

        //[ValidateNever]
        //public virtual ICollection<ProviderPayViewModel> ProviderPays { get; set; } = [];
    }
}
