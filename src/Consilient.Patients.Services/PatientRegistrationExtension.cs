using Consilient.Patients.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Patients.Services
{
    public static class PatientRegistrationExtension
    {
        public static void RegisterPatientServices(this IServiceCollection services)
        {
            services.AddScoped<IPatientService, PatientService>();
        }
    }
}
