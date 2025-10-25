using Consilient.Data;
using Consilient.Employees.Contracts;
using Consilient.Employees.Contracts.Dtos;
using Consilient.Employees.Contracts.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Employees.Services
{
    public class EmployeeService(ConsilientDbContext dataContext) : IEmployeeService
    {
        private readonly ConsilientDbContext _dataContext = dataContext;

        public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var exists = await _dataContext.Employees.AnyAsync(e => e.Email == request.Email);
                if (exists)
                {
                    throw new InvalidOperationException("An employee with the specified email already exists.");
                }
            }
            var entity = request.Adapt<Employee>();
            _dataContext.Employees.Add(entity);
            await _dataContext.SaveChangesAsync();

            return entity.Adapt<EmployeeDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }
            try
            {
                var affected = await _dataContext.Employees
                    .Where(e => e.EmployeeId == id)
                    .ExecuteDeleteAsync();

                return affected > 0;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete employee. Related data or database constraints may prevent deletion.", ex);
            }
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            var dtos = await _dataContext.Employees
                .AsNoTracking()
                .ProjectToType<EmployeeDto>()
                .ToListAsync();

            return dtos;
        }

        public async Task<EmployeeDto?> GetByEmailAsync(string email)
        {
            ArgumentNullException.ThrowIfNull(email);

            var dto = await _dataContext.Employees
                .AsNoTracking()
                .Where(e => e.Email == email)
                .ProjectToType<EmployeeDto>()
                .FirstOrDefaultAsync();

            return dto;
        }
        public async Task<EmployeeDto?> GetByIdAsync(int id)
        {
            var dto = await _dataContext.Employees
                .AsNoTracking()
                .Where(e => e.EmployeeId == id)
                .ProjectToType<EmployeeDto>()
                .FirstOrDefaultAsync();

            return dto;
        }

        public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var affected = await _dataContext.Employees
                .Where(e => e.EmployeeId == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.FirstName, _ => request.FirstName)
                    .SetProperty(e => e.LastName, _ => request.LastName)
                    .SetProperty(e => e.TitleExtension, _ => request.TitleExtension)
                    .SetProperty(e => e.IsProvider, _ => request.IsProvider)
                    .SetProperty(e => e.Role, _ => request.Role)
                    .SetProperty(e => e.IsAdministrator, _ => request.IsAdministrator)
                    .SetProperty(e => e.CanApproveVisits, _ => request.CanApproveVisits)
                );

            if (affected == 0)
            {
                return null;
            }

            return await _dataContext.Employees
                .AsNoTracking()
                .Where(e => e.EmployeeId == id)
                .ProjectToType<EmployeeDto>()
                .FirstOrDefaultAsync();
        }
    }
}
