using Consilient.Api.Client.Contracts;
using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Dtos;
using System.Net;
using System.Net.Http.Json;

namespace Consilient.Api.Client
{
    internal class FacilitiesApi(HttpClient httpClient) : BaseApi(httpClient), IFacilitiesApi
    {
        public async Task<FacilityDto> CreateAsync(CreateFacilityRequest request)
        {
            var resp = await HttpClient.PostAsJsonAsync(Routes.Create(), request).ConfigureAwait(false);

            if (resp.StatusCode == HttpStatusCode.Conflict)
            {
                var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new InvalidOperationException(content);
            }

            resp.EnsureSuccessStatusCode();

            var dto = await resp.Content.ReadFromJsonAsync<FacilityDto?>().ConfigureAwait(false);
            if (dto == null)
            {
                throw new InvalidOperationException("Server returned an empty response when creating facility.");
            }

            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await HttpClient.DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return resp.IsSuccessStatusCode;
        }

        public Task<IEnumerable<FacilityDto>> GetAllAsync()
        {
            return HttpClient.GetFromJsonAsync<IEnumerable<FacilityDto>>(Routes.GetAll())!;
        }

        public Task<FacilityDto?> GetByIdAsync(int id)
        {
            return HttpClient.GetFromJsonAsync<FacilityDto?>(Routes.GetById(id));
        }

        public async Task<FacilityDto?> UpdateAsync(int id, UpdateFacilityRequest request)
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

            return await resp.Content.ReadFromJsonAsync<FacilityDto?>().ConfigureAwait(false);
        }

        static class Routes
        {
            public const string Base = "/facilities";

            public static string Create() => Base;

            public static string Delete(int id) => $"{Base}/{id}";

            public static string GetAll() => Base;
            public static string GetById(int id) => $"{Base}/{id}";

            public static string Update(int id) => $"{Base}/{id}";
        }
    }
}