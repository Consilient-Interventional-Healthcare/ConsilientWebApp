using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Models;
using Consilient.Insurances.Contracts.Dtos;
using Consilient.Insurances.Contracts.Requests;

namespace Consilient.Api.Client.Modules
{
    internal class InsurancesApi(HttpClient client) : BaseApi(client), IInsurancesApi
    {
        public async Task<ApiResponse<InsuranceDto>> CreateAsync(CreateInsuranceRequest request)
        {
            var resp = await PostAsync(Routes.Create(), request).ConfigureAwait(false);
            return await CreateApiResponse<InsuranceDto>(resp);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var resp = await DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return await CreateApiResponse<bool>(resp);
        }

        public async Task<ApiResponse<IEnumerable<InsuranceDto>>> GetAllAsync()
        {
            var resp = await GetAsync(Routes.GetAll()).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<InsuranceDto>>(resp);
        }

        public async Task<ApiResponse<InsuranceDto?>> GetByIdAsync(int id)
        {
            var resp = await GetAsync(Routes.GetById(id)).ConfigureAwait(false);
            return await CreateApiResponse<InsuranceDto?>(resp);
        }

        public async Task<ApiResponse<InsuranceDto>> UpdateAsync(int id, UpdateInsuranceRequest request)
        {
            var resp = await PutAsync(Routes.Update(id), request).ConfigureAwait(false);
            return await CreateApiResponse<InsuranceDto>(resp);
        }

        private static class Routes
        {
            private const string _base = "/insurances";

            public static string Create() => _base;
            public static string Delete(int id) => $"{_base}/{id}";
            public static string GetAll() => _base;
            public static string GetById(int id) => $"{_base}/{id}";
            public static string Update(int id) => $"{_base}/{id}";
        }
    }
}
