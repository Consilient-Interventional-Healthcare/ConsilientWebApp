using ConsilientWebApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsilientWebApp.ViewModels
{
    public class ServiceTypeViewModel
    {
        public int ServiceTypeId { get; set; }

        public string? Description { get; set; }

        [Display(Name = "CPT Code")]
        public int? Cptcode { get; set; }

        [Display(Name = "Code and Description")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? CodeAndDescription { get; set; } = null!;

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
