namespace ConsilientWebApp.ViewModels
{
    public class PatientVisitsIndexViewModel
    {
        public List<ConsilientWebApp.ViewModels.PatientVisitViewModel> PatientVisits = new List<PatientVisitViewModel>();

        public Dictionary<string, int> PhysicianSummaries { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> NursePractitionerSummaries { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ScribeSummaries { get; set; } = new Dictionary<string, int>();
    }
}
