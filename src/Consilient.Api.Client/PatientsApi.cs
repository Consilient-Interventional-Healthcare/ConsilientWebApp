using Consilient.Api.Client.Contracts;
using Consilient.Patients.Contracts.Dtos;
using System.Net.Http.Json;

namespace Consilient.Api.Client
{
    internal class PatientsApi(HttpClient httpClient) : IPatientsApi
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public Task<PatientDto?> GetByMrnAsync(int mrn)
        {
            return _httpClient.GetFromJsonAsync<PatientDto?>($"/patients/{mrn}");
        }
    }
}
