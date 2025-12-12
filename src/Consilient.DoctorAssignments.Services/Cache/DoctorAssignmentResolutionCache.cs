using Consilient.Data;

namespace Consilient.DoctorAssignments.Services.Cache
{
    public class DoctorAssignmentResolutionCache
    {
        private readonly ConsilientDbContext _dbContext;
        private readonly Lazy<HashSet<int>> _lazyFacilityIds;
        private readonly Lazy<Dictionary<int, (string FirstName, string LastName, string? TitleExtension, bool IsProvider)>> _lazyEmployeeDetailsById;
        private readonly Lazy<Dictionary<string, List<int>>> _lazyEmployeeIdsByFullName;
        private readonly Lazy<Dictionary<string, List<int>>> _lazyEmployeeIdsByFullNameWithTitle;
        private readonly Lazy<Dictionary<string, List<int>>> _lazyEmployeeIdsByFullNameWithTitleComma;
        private readonly Lazy<Dictionary<(string mrn, int facilityId), int>> _lazyPatientIdByMrnAndFacility;
        private readonly Lazy<Dictionary<(int caseId, int facilityId), List<(int hospitalizationId, int patientId)>>> _lazyHospitalizationsByCaseIdAndFacility;

        public DoctorAssignmentResolutionCache(ConsilientDbContext dbContext)
        {
            _dbContext = dbContext;
            _lazyFacilityIds = new(() => LoadFacilityIds());
            _lazyEmployeeDetailsById = new(() => LoadEmployeeDetails());
            _lazyEmployeeIdsByFullName = new(() => LoadEmployeeIdsByFullName());
            _lazyEmployeeIdsByFullNameWithTitle = new(() => LoadEmployeeIdsByFullNameWithTitle());
            _lazyEmployeeIdsByFullNameWithTitleComma = new(() => LoadEmployeeIdsByFullNameWithTitleComma());
            _lazyPatientIdByMrnAndFacility = new(() => LoadPatientIdByMrnAndFacility());
            _lazyHospitalizationsByCaseIdAndFacility = new(() => LoadHospitalizationsByCaseIdAndFacility());
        }

        public HashSet<int> FacilityIds => _lazyFacilityIds.Value;
        public Dictionary<int, (string FirstName, string LastName, string? TitleExtension, bool IsProvider)> EmployeeDetailsById => _lazyEmployeeDetailsById.Value;
        public Dictionary<string, List<int>> EmployeeIdsByFullName => _lazyEmployeeIdsByFullName.Value;
        public Dictionary<string, List<int>> EmployeeIdsByFullNameWithTitle => _lazyEmployeeIdsByFullNameWithTitle.Value;
        public Dictionary<string, List<int>> EmployeeIdsByFullNameWithTitleComma => _lazyEmployeeIdsByFullNameWithTitleComma.Value;
        public Dictionary<(string mrn, int facilityId), int> PatientIdByMrnAndFacility => _lazyPatientIdByMrnAndFacility.Value;
        public Dictionary<(int caseId, int facilityId), List<(int hospitalizationId, int patientId)>> HospitalizationsByCaseIdAndFacility => _lazyHospitalizationsByCaseIdAndFacility.Value;

        private HashSet<int> LoadFacilityIds()
        {
            var facilities = _dbContext.Facilities.ToList();
            return [.. facilities.Select(f => f.Id)];
        }

        private Dictionary<int, (string FirstName, string LastName, string? TitleExtension, bool IsProvider)> LoadEmployeeDetails()
        {
            var employees = _dbContext.Employees.ToList();
            var result = new Dictionary<int, (string, string, string?, bool)>();

            foreach (var employee in employees)
            {
                result[employee.Id] = (
                    employee.FirstName ?? string.Empty,
                    employee.LastName ?? string.Empty,
                    employee.TitleExtension,
                    employee.IsProvider
                );
            }

            return result;
        }

        private Dictionary<string, List<int>> LoadEmployeeIdsByFullName()
        {
            return BuildEmployeeNameDictionary(employee =>
                $"{employee.FirstName} {employee.LastName}".Trim(),
                includeOnlyIfNotEmpty: true);
        }

        private Dictionary<string, List<int>> LoadEmployeeIdsByFullNameWithTitle()
        {
            return BuildEmployeeNameDictionary(employee =>
            {
                if (string.IsNullOrEmpty(employee.TitleExtension))
                    return null!;
                var fullName = $"{employee.FirstName} {employee.LastName}".Trim();
                return $"{fullName} {employee.TitleExtension}";
            },
            includeOnlyIfNotEmpty: false);
        }

        private Dictionary<string, List<int>> LoadEmployeeIdsByFullNameWithTitleComma()
        {
            return BuildEmployeeNameDictionary(employee =>
            {
                if (string.IsNullOrEmpty(employee.TitleExtension))
                    return null!;
                var fullName = $"{employee.FirstName} {employee.LastName}".Trim();
                return $"{fullName}, {employee.TitleExtension}";
            },
            includeOnlyIfNotEmpty: false);
        }

        /// <summary>
        /// Generic builder for employee name-based dictionaries. Reduces code duplication
        /// by handling the common pattern of mapping employee names to lists of IDs.
        /// </summary>
        private Dictionary<string, List<int>> BuildEmployeeNameDictionary(
            Func<Data.Entities.Employee, string?> keySelector,
            bool includeOnlyIfNotEmpty = false)
        {
            var employees = _dbContext.Employees.ToList();
            var result = new Dictionary<string, List<int>>();

            foreach (var employee in employees)
            {
                var key = keySelector(employee);

                if (key == null || (includeOnlyIfNotEmpty && string.IsNullOrEmpty(key)))
                    continue;

                if (!result.TryGetValue(key, out var value))
                {
                    value = [];
                    result[key] = value;
                }

                value.Add(employee.Id);
            }

            return result;
        }

        private Dictionary<(string mrn, int facilityId), int> LoadPatientIdByMrnAndFacility()
        {
            var patientFacilities = _dbContext.PatientFacilities.ToList();
            var result = new Dictionary<(string mrn, int facilityId), int>();

            foreach (var pf in patientFacilities)
            {
                result[(pf.Mrn, pf.FacilityId)] = pf.PatientId;
            }

            return result;
        }

        private Dictionary<(int caseId, int facilityId), List<(int hospitalizationId, int patientId)>> LoadHospitalizationsByCaseIdAndFacility()
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

    }
}
