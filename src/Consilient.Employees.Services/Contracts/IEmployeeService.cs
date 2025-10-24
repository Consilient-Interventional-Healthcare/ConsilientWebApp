using Consilient.Employees.Contracts.Dtos;

namespace Consilient.Employees.Services.Contracts
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetAllAsync();
        Task<EmployeeDto?> GetByEmail(string email);
    }
}
