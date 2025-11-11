namespace Consilient.WebApp.ViewModels
{
    public class PatientVisitsIndexViewModel
    {
        public List<PatientVisitViewModel>? PatientVisits { get; set; } = [];

        public Dictionary<string, int>? PhysicianSummaries { get; set; } = [];
        public Dictionary<string, int>? NursePractitionerSummaries { get; set; } = [];
        public Dictionary<string, int>? ScribeSummaries { get; set; } = [];
    }
}
