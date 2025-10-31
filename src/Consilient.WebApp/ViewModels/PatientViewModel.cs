using System.ComponentModel.DataAnnotations;

namespace Consilient.WebApp.ViewModels
{
    public class PatientViewModel
    {

        [Display(Name = "MRN")]
        public int PatientMrn { get; set; }

        [Display(Name = "Patient First Name")]
        public string? PatientFirstName { get; set; }

        [Display(Name = "Patient Last Name")]
        public string? PatientLastName { get; set; }

        [Display(Name = "Patient Birth Date")]
        public DateOnly? PatientBirthDate { get; set; }

        [Display(Name = "Name")]
        public string PatientFullName { get; set; } = null!;

        //public virtual ICollection<PatientVisitViewModel> PatientVisits { get; set; } = [];

        //public virtual ICollection<PatientVisitsStagingViewModel> PatientVisitsStagings { get; set; } = [];
    }
}
