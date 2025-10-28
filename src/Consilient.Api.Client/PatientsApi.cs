using Consilient.Api.Client.Contracts;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using System.Net.Http.Json;

namespace Consilient.Api.Client
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

        static class Routes
        {
            public const string Base = "/patients";
            public static string Create() => $"{Base}";
            public static string GetAll() => Base;
            public static string GetByMrn(int mrn) => $"{Base}/{mrn}";
        }
    }
}
