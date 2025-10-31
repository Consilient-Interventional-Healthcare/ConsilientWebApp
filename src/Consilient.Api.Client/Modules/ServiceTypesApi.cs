using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Models;
using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;
using System.Net.Http.Json;

namespace Consilient.Api.Client.Modules
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

        private static class Routes
        {
            private const string _base = "/servicetypes";

            public static string Create() => _base;
            public static string Delete(int id) => $"{_base}/{id}";
            public static string GetAll() => _base;
            public static string GetById(int id) => $"{_base}/{id}";
            public static string Update(int id) => $"{_base}/{id}";
        }
    }
}