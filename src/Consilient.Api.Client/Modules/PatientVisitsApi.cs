using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Models;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using System.Net.Http.Json;

namespace Consilient.Api.Client.Modules
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

        public async Task<ApiResponse<IEnumerable<PatientVisitDto>>> GetByDateAsync(DateOnly date)
        {
            var resp = await HttpClient.GetAsync(Routes.GetByDate(date)).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<PatientVisitDto>>(resp);
        }

        public async Task<ApiResponse<IEnumerable<PatientVisitDto>>> GetByEmployeeAsync(int employeeId)
        {
            var resp = await HttpClient.GetAsync(Routes.GetByEmployee(employeeId)).ConfigureAwait(false);
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

        private static class Routes
        {
            private const string _base = "/patients/visits";
            public static string Create() => $"{_base}";
            public static string Delete(int id) => $"{_base}/{id}";
            public static string GetByDate(DateOnly date) => $"{_base}/date/{date:yyyyMMdd}";
            public static string GetByEmployee(int employeeId) => $"{_base}/employee/{employeeId}";
            public static string GetById(int id) => $"{_base}/{id}";
            public static string Update(int id) => $"{_base}/{id}";
        }
    }
}
