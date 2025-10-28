using Consilient.Api.Client.Contracts;
using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;
using System.Net.Http.Json;

namespace Consilient.Api.Client
{
    internal class ServiceTypesApi(HttpClient httpClient) : BaseApi(httpClient), IServiceTypesApi
    {
        public async Task<ApiResponse<ServiceTypeDto?>> CreateAsync(CreateServiceTypeRequest request)
        {
            var resp = await HttpClient.PostAsJsonAsync(Routes.Create(), request).ConfigureAwait(false);
            return await CreateApiResponse<ServiceTypeDto?>(resp);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var resp = await HttpClient.DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return await CreateApiResponse<bool>(resp);
        }

        public async Task<ApiResponse<IEnumerable<ServiceTypeDto>>> GetAllAsync()
        {
            var resp = await HttpClient.GetAsync(Routes.GetAll()).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<ServiceTypeDto>>(resp);
        }

        public async Task<ApiResponse<ServiceTypeDto?>> GetByIdAsync(int id)
        {
            var resp = await HttpClient.GetAsync(Routes.GetById(id)).ConfigureAwait(false);
            return await CreateApiResponse<ServiceTypeDto?>(resp);
        }

        public async Task<ApiResponse<ServiceTypeDto?>> UpdateAsync(int id, UpdateServiceTypeRequest request)
        {
            var resp = await HttpClient.PutAsJsonAsync(Routes.Update(id), request).ConfigureAwait(false);
            return await CreateApiResponse<ServiceTypeDto?>(resp);
        }

        static class Routes
        {
            public const string Base = "/servicetypes";

            public static string Create() => Base;

            public static string Delete(int id) => $"{Base}/{id}";

            public static string GetAll() => Base;
            public static string GetById(int id) => $"{Base}/{id}";

            public static string Update(int id) => $"{Base}/{id}";
        }
    }
}