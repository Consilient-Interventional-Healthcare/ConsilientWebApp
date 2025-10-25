using Consilient.Api.Client.Contracts;
using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;
using System.Net;
using System.Net.Http.Json;

namespace Consilient.Api.Client
{
    internal class ServiceTypesApi(HttpClient httpClient) : BaseApi(httpClient), IServiceTypesApi
    {
        public async Task<ServiceTypeDto> CreateAsync(CreateServiceTypeRequest request)
        {
            var resp = await HttpClient.PostAsJsonAsync(Routes.Create(), request).ConfigureAwait(false);

            if (resp.StatusCode == HttpStatusCode.Conflict)
            {
                var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new InvalidOperationException(content);
            }

            resp.EnsureSuccessStatusCode();

            var dto = await resp.Content.ReadFromJsonAsync<ServiceTypeDto?>().ConfigureAwait(false) ?? throw new InvalidOperationException("Server returned an empty response when creating service type.");
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await HttpClient.DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return resp.IsSuccessStatusCode;
        }

        public Task<IEnumerable<ServiceTypeDto>> GetAllAsync()
        {
            return HttpClient.GetFromJsonAsync<IEnumerable<ServiceTypeDto>>(Routes.GetAll())!;
        }

        public Task<ServiceTypeDto?> GetByIdAsync(int id)
        {
            return HttpClient.GetFromJsonAsync<ServiceTypeDto?>(Routes.GetById(id));
        }

        public async Task<ServiceTypeDto?> UpdateAsync(int id, UpdateServiceTypeRequest request)
        {
            var resp = await HttpClient.PutAsJsonAsync(Routes.Update(id), request).ConfigureAwait(false);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (resp.StatusCode == HttpStatusCode.Conflict)
            {
                var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new InvalidOperationException(content);
            }

            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<ServiceTypeDto?>().ConfigureAwait(false);
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