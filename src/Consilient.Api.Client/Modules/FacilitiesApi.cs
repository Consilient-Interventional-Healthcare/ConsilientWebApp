using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Models;
using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;

namespace Consilient.Api.Client.Modules
{
    internal class FacilitiesApi(HttpClient httpClient) : BaseApi(httpClient), IFacilitiesApi
    {
        public async Task<ApiResponse<FacilityDto?>> CreateAsync(CreateFacilityRequest request)
        {
            var resp = await PostAsync(Routes.Create(), request).ConfigureAwait(false);
            return await CreateApiResponse<FacilityDto?>(resp);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var resp = await DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return await CreateApiResponse<bool>(resp);
        }

        public async Task<ApiResponse<IEnumerable<FacilityDto>>> GetAllAsync()
        {
            var resp = await GetAsync(Routes.GetAll()).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<FacilityDto>>(resp);
        }

        public async Task<ApiResponse<FacilityDto?>> GetByIdAsync(int id)
        {
            var resp = await GetAsync(Routes.GetById(id)).ConfigureAwait(false);
            return await CreateApiResponse<FacilityDto?>(resp);
        }

        public async Task<ApiResponse<FacilityDto?>> UpdateAsync(int id, UpdateFacilityRequest request)
        {
            var resp = await PutAsync(Routes.Update(id), request).ConfigureAwait(false);
            return await CreateApiResponse<FacilityDto?>(resp);
        }

        private static class Routes
        {
            private const string _base = "/facilities";

            public static string Create() => _base;

            public static string Delete(int id) => $"{_base}/{id}";

            public static string GetAll() => _base;

            public static string GetById(int id) => $"{_base}/{id}";

            public static string Update(int id) => $"{_base}/{id}";
        }
    }
}