using Consilient.Api.Client.Contracts;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using System.Net.Http.Json;

namespace Consilient.Api.Client
{
    internal class PatientVisitsApi(HttpClient httpClient) : BaseApi(httpClient), IPatientVisitsApi
    {
        public async Task<ApiResponse<PatientVisitDto?>> CreateAsync(CreatePatientVisitRequest request)
        {
            var resp = await HttpClient.PostAsJsonAsync(Routes.Create(), request).ConfigureAwait(false);
            return await CreateApiResponse<PatientVisitDto?>(resp);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var resp = await HttpClient.DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return await CreateApiResponse<bool>(resp);
        }

        public async Task<ApiResponse<IEnumerable<PatientVisitDto>>> GetByDateAsync(DateTime date)
        {
            var resp = await HttpClient.GetAsync(Routes.GetByDate(date)).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<PatientVisitDto>>(resp);
        }

        public async Task<ApiResponse<PatientVisitDto?>> GetByIdAsync(int id)
        {
            var resp = await HttpClient.GetAsync(Routes.GetById(id)).ConfigureAwait(false);
            return await CreateApiResponse<PatientVisitDto?>(resp);
        }

        public async Task<ApiResponse<PatientVisitDto?>> UpdateAsync(int id, UpdatePatientVisitRequest request)
        {
            var resp = await HttpClient.PutAsJsonAsync(Routes.Update(id), request).ConfigureAwait(false);
            return await CreateApiResponse<PatientVisitDto?>(resp);
        }

        static class Routes
        {
            public const string Base = "/patients/visits";
            public static string Create() => $"{Base}";
            public static string Delete(int id) => $"{Base}/{id}";
            public static string? GetByDate(DateTime date) => $"{Base}/by-date/{date}";
            public static string? GetById(int id) => $"{Base}/{id}";
            public static string Update(int id) => $"{Base}/{id}";
        }
    }
}
