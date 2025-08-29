using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConsilientWebApp.ViewModels
{
    public class PatientVisitsStagingIndexViewModel
    {
        public List<PatientVisitsStagingViewModel> PatientVisitsStaging { get; set; } = new List<PatientVisitsStagingViewModel>();

        public DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-1).Date);
        public int SelectedFacilityId { get; set; }   
        public int SelectedProviderId { get; set; }

        public List<SelectListItem> FacilitiesSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProvidersSelectList { get; set; } = new List<SelectListItem>();

        public Dictionary<string, int> PhysicianSummaries { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> NursePractitionerSummaries { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ScribeSummaries { get; set; } = new Dictionary<string, int>();
    }
}
