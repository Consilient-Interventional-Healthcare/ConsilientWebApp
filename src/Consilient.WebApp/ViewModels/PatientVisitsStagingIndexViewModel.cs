using Consilient.Patients.Contracts.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Consilient.WebApp.ViewModels
{
    public class PatientVisitsStagingIndexViewModel
    {
        public List<StagingPatientVisitDto> PatientVisitsStaging { get; init; } = [];

        public DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-1).Date);
        public int SelectedFacilityId { get; init; }
        public int SelectedProviderId { get; init; }

        public List<SelectListItem> FacilitiesSelectList { get; set; } = [];
        public List<SelectListItem> ProvidersSelectList { get; set; } = [];

        //public Dictionary<string, int> PhysicianSummaries { get; set; } = [];
        //public Dictionary<string, int> NursePractitionerSummaries { get; set; } = [];
        //public Dictionary<string, int> ScribeSummaries { get; set; } = [];
    }
}
