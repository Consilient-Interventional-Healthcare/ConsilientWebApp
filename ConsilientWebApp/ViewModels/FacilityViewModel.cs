using ConsilientWebApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ConsilientWebApp.ViewModels
{
    public class FacilityViewModel
    {
        public int FacilityId { get; set; }

        [Display(Name = "Facility Name")]
        public string? FacilityName { get; set; }

        [Display(Name = "Facility Abbreviation")]
        public string? FacilityAbbreviation { get; set; }

        [ValidateNever]
        public virtual ICollection<ContractViewModel> Contracts { get; set; } = new List<ContractViewModel>();

        [ValidateNever]
        public virtual ICollection<FacilityPayViewModel> FacilityPays { get; set; } = new List<FacilityPayViewModel>();

        [ValidateNever]
        public virtual ICollection<PatientVisitViewModel> PatientVisits { get; set; } = new List<PatientVisitViewModel>();

        [ValidateNever]
        public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagings { get; set; } = new List<PatientVisitsStagingViewModel>();

        [ValidateNever]
        public virtual ICollection<ProviderPayViewModel> ProviderPays { get; set; } = new List<ProviderPayViewModel>();
    }
}
