using Consilient.Api.Client.Models;
using Consilient.Employees.Contracts.Dtos;
using Consilient.Employees.Contracts.Requests;

namespace Consilient.Api.Client.Contracts
{
    public interface IEmployeesApi : IApi
    {
        Task<ApiResponse<EmployeeDto?>> CreateAsync(CreateEmployeeRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<EmployeeDto>>> GetAllAsync();
        Task<ApiResponse<EmployeeDto?>> GetByEmailAsync(string email);
        Task<ApiResponse<EmployeeDto?>> GetByIdAsync(int id);
        Task<ApiResponse<EmployeeDto?>> UpdateAsync(int id, UpdateEmployeeRequest request);
    }
}
