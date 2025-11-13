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

    }
}
