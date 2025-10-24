using Consilient.Employees.Contracts;
using Consilient.Employees.Contracts.Dtos;

namespace Consilient.Api.Client.Contracts
{
    public interface IEmployeesApi : IApi
    {
        Task<IEnumerable<EmployeeDto>> GetAllAsync();

        Task<EmployeeDto?> GetByIdAsync(int id);

        Task<EmployeeDto?> GetByEmailAsync(string email);

        Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request);

        Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeRequest request);

        Task<bool> DeleteAsync(int id);
    }
}
