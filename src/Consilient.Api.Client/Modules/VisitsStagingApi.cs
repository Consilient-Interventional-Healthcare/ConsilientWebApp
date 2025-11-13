using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Models;
using Consilient.Visits.Contracts.Dtos;
using Consilient.Visits.Contracts.Requests;
using Consilient.Visits.Contracts.Results;

namespace Consilient.Api.Client.Modules
{
    internal class VisitsStagingApi(HttpClient httpClient) : BaseApi(httpClient), IVisitsStagingApi
    {
        public async Task<ApiResponse<VisitStagingDto?>> CreateAsync(CreateVisitStagingRequest request)
        {
            var resp = await PostAsync(Routes.Create(), request).ConfigureAwait(false);
            return await CreateApiResponse<VisitStagingDto?>(resp);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var resp = await DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return await CreateApiResponse<bool>(resp);
        }

        public async Task<ApiResponse<IEnumerable<VisitStagingDto>>> GetByDateAsync(DateOnly date)
        {
            var resp = await GetAsync(Routes.GetByDate(date)).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<VisitStagingDto>>(resp);
        }

        public async Task<ApiResponse<IEnumerable<VisitStagingDto>>> GetByEmployeeAsync(int employeeId)
        {
            var resp = await GetAsync(Routes.GetByEmployee(employeeId)).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<VisitStagingDto>>(resp);
        }

        public async Task<ApiResponse<VisitStagingDto?>> GetByIdAsync(int id)
        {
            var resp = await GetAsync(Routes.GetById(id)).ConfigureAwait(false);
            return await CreateApiResponse<VisitStagingDto?>(resp);
        }

        public async Task<ApiResponse<int>> PushApprovedVisitsAsync()
        {
            var resp = await PostAsync(Routes.PushApprovedVisits(), null).ConfigureAwait(false);
            return await CreateApiResponse<int>(resp);
        }

        public async Task<ApiResponse<VisitStagingDto?>> UpdateAsync(int id, UpdateVisitStagingRequest request)
        {
            var resp = await PutAsync(Routes.Update(id), request).ConfigureAwait(false);
            return await CreateApiResponse<VisitStagingDto?>(resp);
        }

        public async Task<ApiResponse<UploadAssignmentResult>> UploadSpreadsheetAsync(Models.File file)
        {
            ArgumentNullException.ThrowIfNull(file);
            var resp = await PostAsync(Routes.UploadSpreadsheet(), null, [file]).ConfigureAwait(false);
            return await CreateApiResponse<UploadAssignmentResult>(resp);
        }
        private static class Routes
        {
            private const string _base = "/visits/staging";
            public static string Create() => $"{_base}";
            public static string Delete(int id) => $"{_base}/{id}";
            public static string GetByDate(DateOnly date) => $"{_base}/date/{date:yyyyMMdd}";
            public static string GetByEmployee(int employeeId) => $"{_base}/employee/{employeeId}";
            public static string GetById(int id) => $"{_base}/{id}";
            public static string PushApprovedVisits() => $"{_base}/push-approved";
            public static string Update(int id) => $"{_base}/{id}";
            public static string UploadSpreadsheet() => $"{_base}/upload-spreadsheet";
        }
    }
}
