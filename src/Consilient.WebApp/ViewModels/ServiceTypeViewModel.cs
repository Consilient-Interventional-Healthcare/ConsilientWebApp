using System.ComponentModel.DataAnnotations;

namespace Consilient.WebApp.ViewModels
{
    public class ServiceTypeViewModel
    {
        public int ServiceTypeId { get; init; }

        public string? Description { get; init; }

        [Display(Name = "CPT Code")]
        public int? Cptcode { get; init; }

        [Display(Name = "Code and Description")]
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? CodeAndDescription { get; init; } 

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
