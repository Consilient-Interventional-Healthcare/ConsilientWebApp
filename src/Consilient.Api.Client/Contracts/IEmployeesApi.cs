using Consilient.Employees.Contracts.Dtos;

namespace Consilient.Api.Client.Contracts
{
    public interface IEmployeesApi
    {
        public Task<IEnumerable<EmployeeDto>> GetAllAsync();
    }
}
