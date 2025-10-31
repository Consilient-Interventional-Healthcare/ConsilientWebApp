using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Models;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using System.Net.Http.Json;

namespace Consilient.Api.Client.Modules
{
    internal class PatientsApi(HttpClient httpClient) : BaseApi(httpClient), IPatientsApi
    {
        public async Task<ApiResponse<PatientDto?>> CreateAsync(CreatePatientRequest request)
        {
            var resp = await HttpClient.PostAsJsonAsync(Routes.Create(), request).ConfigureAwait(false);
            return await CreateApiResponse<PatientDto?>(resp);
        }

        public async Task<ApiResponse<IEnumerable<PatientDto>>> GetAllAsync()
        {
            var resp = await HttpClient.GetAsync(Routes.GetAll()).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<PatientDto>>(resp);
        }

        public async Task<ApiResponse<PatientDto?>> GetByMrnAsync(int mrn)
        {
            var resp = await HttpClient.GetAsync(Routes.GetByMrn(mrn)).ConfigureAwait(false);
            return await CreateApiResponse<PatientDto?>(resp);
        }

        private static class Routes
        {
            private const string _base = "/patients";
            public static string Create() => $"{_base}";
            public static string GetAll() => _base;
            public static string GetByMrn(int mrn) => $"{_base}/{mrn}";
        }
    }
}
