namespace Consilient.WebApp.ViewModels
{
    public class PatientVisitsIndexViewModel
    {
        public List<Consilient.WebApp.ViewModels.PatientVisitViewModel> PatientVisits = [];

        public Dictionary<string, int> PhysicianSummaries { get; set; } = [];
        public Dictionary<string, int> NursePractitionerSummaries { get; set; } = [];
        public Dictionary<string, int> ScribeSummaries { get; set; } = [];
    }
}
