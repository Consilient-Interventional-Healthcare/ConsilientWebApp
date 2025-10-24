using Consilient.Data;
using Consilient.Employees.Contracts.Dtos;
using Consilient.Employees.Services.Contracts;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Employees.Services
{
    public class EmployeeService(ConsilientDbContext dataContext) : IEmployeeService
    {
        private readonly ConsilientDbContext dataContext = dataContext;

        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            var employees = await dataContext.Employees.ToListAsync();
            return employees.Adapt<IEnumerable<EmployeeDto>>();
        }

        public async Task<EmployeeDto?> GetByEmail(string email)
        {
            ArgumentNullException.ThrowIfNull(email);
            var employee = await dataContext.Employees.FirstOrDefaultAsync(e => e.Email == email);
            return employee?.Adapt<EmployeeDto>();
        }
    }
}
