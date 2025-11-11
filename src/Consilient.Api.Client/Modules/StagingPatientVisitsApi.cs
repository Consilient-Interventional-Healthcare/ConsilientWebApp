using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Models;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using Consilient.Patients.Contracts.Results;

namespace Consilient.Api.Client.Modules
{
    internal class StagingPatientVisitsApi(HttpClient httpClient) : BaseApi(httpClient), IStagingPatientVisitsApi
    {
        public async Task<ApiResponse<StagingPatientVisitDto?>> CreateAsync(CreateStagingPatientVisitRequest request)
        {
            var resp = await PostAsync(Routes.Create(), request).ConfigureAwait(false);
            return await CreateApiResponse<StagingPatientVisitDto?>(resp);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var resp = await DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return await CreateApiResponse<bool>(resp);
        }

        public async Task<ApiResponse<IEnumerable<StagingPatientVisitDto>>> GetByDateAsync(DateOnly date)
        {
            var resp = await GetAsync(Routes.GetByDate(date)).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<StagingPatientVisitDto>>(resp);
        }

        public async Task<ApiResponse<IEnumerable<StagingPatientVisitDto>>> GetByEmployeeAsync(int employeeId)
        {
            var resp = await GetAsync(Routes.GetByEmployee(employeeId)).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<StagingPatientVisitDto>>(resp);
        }

        public async Task<ApiResponse<StagingPatientVisitDto?>> GetByIdAsync(int id)
        {
            var resp = await GetAsync(Routes.GetById(id)).ConfigureAwait(false);
            return await CreateApiResponse<StagingPatientVisitDto?>(resp);
        }

        public async Task<ApiResponse<int>> PushApprovedPatientVisitsAsync()
        {
            var resp = await PostAsync(Routes.PushApprovedPatientVisits(), null).ConfigureAwait(false);
            return await CreateApiResponse<int>(resp);
        }

        public async Task<ApiResponse<StagingPatientVisitDto?>> UpdateAsync(int id, UpdateStagingPatientVisitRequest request)
        {
            var resp = await PutAsync(Routes.Update(id), request).ConfigureAwait(false);
            return await CreateApiResponse<StagingPatientVisitDto?>(resp);
        }

        public async Task<ApiResponse<UploadSpreadsheetResult>> UploadSpreadsheetAsync(Models.File file)
        {
            ArgumentNullException.ThrowIfNull(file);
            var resp = await PostAsync(Routes.UploadSpreadsheet(), null, [file]).ConfigureAwait(false);
            return await CreateApiResponse<UploadSpreadsheetResult>(resp);
        }
        private static class Routes
        {
            private const string _base = "/patients/visits/staging";
            public static string Create() => $"{_base}";
            public static string Delete(int id) => $"{_base}/{id}";
            public static string GetByDate(DateOnly date) => $"{_base}/date/{date:yyyyMMdd}";
            public static string GetByEmployee(int employeeId) => $"{_base}/employee/{employeeId}";
            public static string GetById(int id) => $"{_base}/{id}";
            public static string PushApprovedPatientVisits() => $"{_base}/push-approved";
            public static string Update(int id) => $"{_base}/{id}";
            public static string UploadSpreadsheet() => $"{_base}/upload-spreadsheet";
        }
    }
}
