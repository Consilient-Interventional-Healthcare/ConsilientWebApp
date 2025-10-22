using Microsoft.AspNetCore.Mvc.Rendering;

namespace Consilient.WebApp.ViewModels
{
    public class PatientVisitsStagingIndexViewModel
    {
        public List<PatientVisitsStagingViewModel> PatientVisitsStaging { get; set; } = [];

        public DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-1).Date);
        public int SelectedFacilityId { get; set; }
        public int SelectedProviderId { get; set; }

        public List<SelectListItem> FacilitiesSelectList { get; set; } = [];
        public List<SelectListItem> ProvidersSelectList { get; set; } = [];

        public Dictionary<string, int> PhysicianSummaries { get; set; } = [];
        public Dictionary<string, int> NursePractitionerSummaries { get; set; } = [];
        public Dictionary<string, int> ScribeSummaries { get; set; } = [];
    }
}
