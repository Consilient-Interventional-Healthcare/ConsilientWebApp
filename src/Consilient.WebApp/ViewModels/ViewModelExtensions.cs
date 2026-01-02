namespace Consilient.WebApp.ViewModels
{
    public static class ViewModelExtensions
    {
        public static string GetClass(this PatientVisitsStagingViewModel model)
        {
            return model.PhysicianApproved && model.NursePractitionerApproved
                ? "bg-green-400"
                : model.PhysicianApproved && model.NursePractitionerEmployeeId == null
                    ? "bg-green-400"
                    : model.PhysicianApproved || model.NursePractitionerApproved
                        ? "bg-green-50"
                        : "bg-white";
        }
    }
}
