using Consilient.Api.Client.Contracts;
using Consilient.Patients.Contracts.Dtos;
using System.Net.Http.Json;

namespace Consilient.Api.Client
{
    internal class PatientsApi(HttpClient httpClient) : BaseApi(httpClient), IPatientsApi
    {
        public Task<PatientDto?> GetByMrnAsync(int mrn)
        {
            return HttpClient.GetFromJsonAsync<PatientDto?>(Routes.GetByMrn(mrn));
        }

        static class Routes
        {
            public const string Base = "/patients";

            public static string GetByMrn(int mrn) => $"{Base}/{mrn}";
        }
    }
}
