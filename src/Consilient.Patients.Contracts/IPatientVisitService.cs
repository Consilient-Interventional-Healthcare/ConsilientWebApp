using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;

namespace Consilient.Patients.Contracts
{
    public interface IPatientVisitService<TPatientVisit, in TCreatePatientVisit, in TUpdatePatientVisit>
        where TPatientVisit : class, new()
        where TCreatePatientVisit : class, new()
        where TUpdatePatientVisit : class, new()
    {
        Task<IEnumerable<TPatientVisit>> GetByDateAsync(DateOnly date);
        Task<TPatientVisit?> GetByIdAsync(int id);
        Task<IEnumerable<TPatientVisit>> GetByProviderAsync(int providerId);
        Task<TPatientVisit?> CreateAsync(TCreatePatientVisit request);
        Task<TPatientVisit?> UpdateAsync(int id, TUpdatePatientVisit request);
        Task<bool> DeleteAsync(int id);
    }

    public interface IPatientVisitService : IPatientVisitService<PatientVisitDto, CreatePatientVisitRequest,
        UpdatePatientVisitRequest>;
}
