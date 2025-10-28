using Consilient.Api.Client.Contracts;
using Consilient.Employees.Contracts.Dtos;
using Consilient.Employees.Contracts.Requests;
using System.Net.Http.Json;

namespace Consilient.Api.Client
{
    internal class EmployeesApi(HttpClient httpClient) : BaseApi(httpClient), IEmployeesApi
    {
        public async Task<ApiResponse<EmployeeDto?>> CreateAsync(CreateEmployeeRequest request)
        {
            var resp = await HttpClient.PostAsJsonAsync(Routes.Create(), request).ConfigureAwait(false);
            return await CreateApiResponse<EmployeeDto?>(resp);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var resp = await HttpClient.DeleteAsync(Routes.Delete(id)).ConfigureAwait(false);
            return await CreateApiResponse<bool>(resp);
        }

        public async Task<ApiResponse<IEnumerable<EmployeeDto>>> GetAllAsync()
        {
            var resp = await HttpClient.GetAsync(Routes.GetAll()).ConfigureAwait(false);
            return await CreateApiResponse<IEnumerable<EmployeeDto>>(resp);
        }

        public async Task<ApiResponse<EmployeeDto?>> GetByEmailAsync(string email)
        {
            var resp = await HttpClient.GetAsync(Routes.GetByEmail(email)).ConfigureAwait(false);
            return await CreateApiResponse<EmployeeDto?>(resp);
        }

        public async Task<ApiResponse<EmployeeDto?>> GetByIdAsync(int id)
        {
            var resp = await HttpClient.GetAsync(Routes.GetById(id)).ConfigureAwait(false);
            return await CreateApiResponse<EmployeeDto?>(resp);
        }

        public async Task<ApiResponse<EmployeeDto?>> UpdateAsync(int id, UpdateEmployeeRequest request)
        {
            var resp = await HttpClient.PutAsJsonAsync(Routes.Update(id), request).ConfigureAwait(false);
            return await CreateApiResponse<EmployeeDto?>(resp);
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