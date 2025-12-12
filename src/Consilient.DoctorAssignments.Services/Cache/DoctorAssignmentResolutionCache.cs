using Consilient.Common;
using Consilient.Data;
using Microsoft.EntityFrameworkCore;

namespace Consilient.DoctorAssignments.Services.Cache
{
    public record EmployeeContractRow(
        int Id,
        string LastName,
        string FirstName,
        EmployeeRole Role,
        int? ProviderContractId,
        DateOnly? StartDate,
        DateOnly? EndDate,
        int? FacilityId);

    public class DoctorAssignmentResolutionCache
    {
        private readonly ConsilientDbContext _dbContext;
        private readonly Lazy<List<EmployeeContractRow>> _lazyAllEmployees;
        private readonly Lazy<HashSet<int>> _lazyFacilities;
        private readonly Lazy<Dictionary<(int caseId, int facilityId), List<(int hospitalizationId, int patientId)>>> _lazyHospitalizations;
        private readonly Lazy<Dictionary<(string mrn, int facilityId), int>> _lazyPatients;
        public DoctorAssignmentResolutionCache(ConsilientDbContext dbContext)
        {
            _dbContext = dbContext;
            _lazyFacilities = new(LoadFacilities);
            _lazyAllEmployees = new(LoadEmployees);
            _lazyPatients = new(LoadPatients);
            _lazyHospitalizations = new(LoadHospitalizations);
        }

        public List<EmployeeContractRow> Employees => _lazyAllEmployees.Value;
        public HashSet<int> Facilities => _lazyFacilities.Value;
        public Dictionary<(int caseId, int facilityId), List<(int hospitalizationId, int patientId)>> Hospitalizations => _lazyHospitalizations.Value;
        public Dictionary<(string mrn, int facilityId), int> Patients => _lazyPatients.Value;

        private List<EmployeeContractRow> LoadEmployees()
        {
            var rows = _dbContext.Database.SqlQueryRaw<EmployeeContractRow>(@"
                SELECT
                    E.Id AS EmployeeId,
                    E.LastName,
                    E.FirstName,
                    E.Role,
                    PC.Id AS ProviderContractId,
                    PC.StartDate,
                    PC.EndDate,
                    PC.FacilityId
                FROM Compensation.Employees AS E
                LEFT JOIN Compensation.ProviderContracts AS PC 
                    ON E.Id = PC.EmployeeId
                WHERE E.Role = {Role1} OR E.Role = {Role2}", new { Role1 = (int)EmployeeRole.Provider, Role2 = (int)EmployeeRole.NursePractitioner }).ToList();
            return rows;
        }

        private HashSet<int> LoadFacilities()
        {
            var facilities = _dbContext.Facilities.Select(m => m.Id).ToHashSet();
            return facilities;
        }


        private Dictionary<(int caseId, int facilityId), List<(int hospitalizationId, int patientId)>> LoadHospitalizations()
        {
            var hospitalizations = _dbContext.Hospitalizations.ToList();
            var result = new Dictionary<(int, int), List<(int, int)>>();

            foreach (var hospitalization in hospitalizations)
            {
                var key = (hospitalization.CaseId, hospitalization.FacilityId);
                if (!result.TryGetValue(key, out var list))
                {
                    list = [];
                    result[key] = list;
                }
                list.Add((hospitalization.Id, hospitalization.PatientId));
            }

            return result;
        }

        private Dictionary<(string mrn, int facilityId), int> LoadPatients()
        {
            var patientFacilities = _dbContext.PatientFacilities.ToList();
            var result = new Dictionary<(string mrn, int facilityId), int>();

            foreach (var pf in patientFacilities)
            {
                result[(pf.Mrn, pf.FacilityId)] = pf.PatientId;
            }

            return result;
        }
    }
}
