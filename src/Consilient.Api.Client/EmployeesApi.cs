using Consilient.Api.Client.Contracts;
using Consilient.Employees.Contracts;
using Consilient.Employees.Contracts.Dtos;
using System.Net;
using System.Net.Http.Json;

namespace Consilient.Api.Client
{
    internal class EmployeesApi(HttpClient httpClient) : BaseApi(httpClient), IEmployeesApi
    {
        public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request)
        {
            var resp = await HttpClient.PostAsJsonAsync(Routes.Create(), request).ConfigureAwait(false);

            if (resp.StatusCode == HttpStatusCode.Conflict)
            {
                var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new InvalidOperationException(content);
            }

            resp.EnsureSuccessStatusCode();

            var dto = await resp.Content.ReadFromJsonAsync<EmployeeDto?>().ConfigureAwait(false);
            if (dto == null)
            {
                throw new InvalidOperationException("Server returned an empty response when creating employee.");
            }

            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await HttpClient.DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return resp.IsSuccessStatusCode;
        }

        public Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            return HttpClient.GetFromJsonAsync<IEnumerable<EmployeeDto>>(Routes.GetAll())!;
        }

        public Task<EmployeeDto?> GetByEmailAsync(string email)
        {
            return HttpClient.GetFromJsonAsync<EmployeeDto?>(Routes.GetByEmail(email));
        }

        public Task<EmployeeDto?> GetByIdAsync(int id)
        {
            return HttpClient.GetFromJsonAsync<EmployeeDto?>(Routes.GetById(id));
        }

        public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeRequest request)
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

            return await resp.Content.ReadFromJsonAsync<EmployeeDto?>().ConfigureAwait(false);
        }

        static class Routes
        {
            public const string Base = "/employees";

            public static string Create() => Base;

            public static string Delete(int id) => $"{Base}/{id}";

            public static string GetAll() => Base;

            public static string GetByEmail(string email) => $"{Base}/email/{Uri.EscapeDataString(email)}";

            public static string GetById(int id) => $"{Base}/{id}";

            public static string Update(int id) => $"{Base}/{id}";
        }
    }
}