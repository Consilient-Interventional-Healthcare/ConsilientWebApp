using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Models;
using Consilient.Visits.Contracts.Models;
using Consilient.Visits.Contracts.Models.Requests;

namespace Consilient.Api.Client.Modules
{
    internal class VisitsApi(HttpClient httpClient) : BaseApi(httpClient), IVisitsApi
    {
        public async Task<ApiResponse<VisitDto?>> CreateAsync(CreateVisitRequest request)
        {
            var resp = await PostAsync(Routes.Create(), request).ConfigureAwait(false);
            return await CreateApiResponse<VisitDto?>(resp);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var resp = await DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return await CreateApiResponse<bool>(resp);
        }

        public async Task<ApiResponse<IEnumerable<VisitDto>>> GetByDateAsync(DateOnly date)
        {
            var resp = await GetAsync(Routes.GetByDate(date)).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<VisitDto>>(resp);
        }

        public async Task<ApiResponse<IEnumerable<VisitDto>>> GetByProviderAsync(int providerId)
        {
            var resp = await GetAsync(Routes.GetByProvider(providerId)).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<VisitDto>>(resp);
        }

        public async Task<ApiResponse<VisitDto?>> GetByIdAsync(int id)
        {
            var resp = await GetAsync(Routes.GetById(id)).ConfigureAwait(false);
            return await CreateApiResponse<VisitDto?>(resp);
        }

        public async Task<ApiResponse<VisitDto?>> UpdateAsync(int id, UpdateVisitRequest request)
        {
            var resp = await PutAsync(Routes.Update(id), request).ConfigureAwait(false);
            return await CreateApiResponse<VisitDto?>(resp);
        }

        private static class Routes
        {
            private const string _base = "/visits";
            public static string Create() => $"{_base}";
            public static string Delete(int id) => $"{_base}/{id}";
            public static string GetByDate(DateOnly date) => $"{_base}/date/{date:yyyyMMdd}";
            public static string GetByProvider(int providerId) => $"{_base}/provider/{providerId}";
            public static string GetById(int id) => $"{_base}/{id}";
            public static string Update(int id) => $"{_base}/{id}";
        }
    }
}
