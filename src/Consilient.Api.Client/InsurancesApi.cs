using Consilient.Api.Client.Contracts;
using Consilient.Insurances.Contracts.Dtos;
using Consilient.Insurances.Contracts.Requests;
using System.Net.Http.Json;

namespace Consilient.Api.Client
{
    internal class InsurancesApi(HttpClient client) : BaseApi(client), IInsurancesApi
    {
        public async Task<ApiResponse<InsuranceDto>> CreateAsync(CreateInsuranceRequest request)
        {
            var resp = await HttpClient.PostAsJsonAsync(Routes.Create(), request).ConfigureAwait(false);
            return await CreateApiResponse<InsuranceDto>(resp);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var resp = await HttpClient.DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return await CreateApiResponse<bool>(resp);
        }

        public async Task<ApiResponse<IEnumerable<InsuranceDto>>> GetAllAsync()
        {
            var resp = await HttpClient.GetAsync(Routes.GetAll()).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<InsuranceDto>>(resp);
        }

        public async Task<ApiResponse<InsuranceDto?>> GetByIdAsync(int id)
        {
            var resp = await HttpClient.GetAsync(Routes.GetById(id)).ConfigureAwait(false);
            return await CreateApiResponse<InsuranceDto?>(resp);
        }

        public async Task<ApiResponse<InsuranceDto>> UpdateAsync(int id, UpdateInsuranceRequest request)
        {
            var resp = await HttpClient.PutAsJsonAsync(Routes.Update(id), request).ConfigureAwait(false);
            return await CreateApiResponse<InsuranceDto>(resp);
        }

        static class Routes
        {
            public const string Base = "/insurances";

            public static string Create() => Base;
            public static string Delete(int id) => $"{Base}/{id}";
            public static string GetAll() => Base;
            public static string GetById(int id) => $"{Base}/{id}";
            public static string Update(int id) => $"{Base}/{id}";
        }
    }
}
