using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.Employees.Contracts;
using Consilient.Employees.Contracts.Dtos;
using Consilient.Employees.Contracts.Requests;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Employees.Services
{
    public class EmployeeService(ConsilientDbContext dataContext) : IEmployeeService
    {
        public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var exists = await dataContext.Employees.AnyAsync(e => e.Email == request.Email);
                if (exists)
                {
                    throw new InvalidOperationException("An employee with the specified email already exists.");
                }
            }
            var entity = request.Adapt<Employee>();
            dataContext.Employees.Add(entity);
            await dataContext.SaveChangesAsync();

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
                var affected = await dataContext.Employees
                    .Where(e => e.Id == id)
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
            var dtos = await dataContext.Employees
                .AsNoTracking()
                .ProjectToType<EmployeeDto>()
                .ToListAsync();

            return dtos;
        }

        public async Task<EmployeeDto?> GetByEmailAsync(string email)
        {
            ArgumentNullException.ThrowIfNull(email);

            var dto = await dataContext.Employees
                .AsNoTracking()
                .Where(e => e.Email == email)
                .ProjectToType<EmployeeDto>()
                .FirstOrDefaultAsync();

            return dto;
        }
        public async Task<EmployeeDto?> GetByIdAsync(int id)
        {
            var employee = await dataContext.Employees.FindAsync(id);
            return employee?.Adapt<EmployeeDto>();
        }

        public async Task<List<EmployeeVisitCountDto>> GetEmployeesWithVisitCountPerDayAsync(DateOnly date)
        {
            var result = await dataContext.Database.SqlQueryRaw<EmployeeVisitCountDto>(@"
                SELECT * FROM (
                    SELECT
                        PR.Id AS ProviderId,
                        PR.LastName AS ProviderLastName,
                        PR.FirstName AS ProviderFirstName,
                        PR.Type AS ProviderType,
                        F.Id AS FacilityId,
                        F.Abbreviation AS FacilityAbbreviation,
                        P.Id AS PatientId,
                        PF.Mrn AS PatientMRN,
                        P.LastName AS PatientLastName,
                        P.FirstName AS PatientFirstName,
                        V.Id AS VisitId,
                        V.DateServiced,
                        V.Room,
                        V.Bed
                    FROM Clinical.Providers AS PR
                    INNER JOIN Clinical.VisitAttendants AS VA
                        ON PR.Id = VA.ProviderId
                    INNER JOIN Clinical.Visits AS V
                        ON VA.VisitId = V.Id
                    INNER JOIN Clinical.Hospitalizations AS H
                        ON H.Id = V.HospitalizationId
                    INNER JOIN Clinical.Facilities AS F
                        ON F.Id = H.FacilityId
                    INNER JOIN Clinical.Patients AS P
                        ON P.Id = H.PatientId
                    INNER JOIN Clinical.PatientFacilities AS PF
                        ON PF.PatientId = P.Id AND PF.FacilityId = F.Id
                    WHERE V.DateServiced = @date
                ) AS Result
                ORDER BY DateServiced, ProviderLastName, ProviderFirstName, PatientLastName, PatientFirstName
            ", new SqlParameter("@date", date)).ToListAsync();
            return result;
        }

        public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var affected = await dataContext.Employees
                .Where(e => e.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.FirstName, _ => request.FirstName)
                    .SetProperty(e => e.LastName, _ => request.LastName)
                    .SetProperty(e => e.TitleExtension, _ => request.TitleExtension)
                    //.SetProperty(e => e.IsProvider, _ => request.IsProvider)
                    .SetProperty(e => e.Role, _ => request.Role)
                    //.SetProperty(e => e.IsAdministrator, _ => request.IsAdministrator)
                    //.SetProperty(e => e.CanApproveVisits, _ => request.CanApproveVisits)
                );

            if (affected == 0)
            {
                return null;
            }

            return await GetByIdAsync(id);
        }
    }
}
