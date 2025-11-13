using Consilient.Visits.Contracts.Dtos;
using Consilient.Visits.Contracts.Requests;

namespace Consilient.Visits.Contracts
{
    public interface IVisitService<TVisit, in TCreateVisit, in TUpdateVisit>
        where TVisit : class, new()
        where TCreateVisit : class, new()
        where TUpdateVisit : class, new()
    {
        Task<IEnumerable<TVisit>> GetByDateAsync(DateOnly date);
        Task<TVisit?> GetByIdAsync(int id);
        Task<IEnumerable<TVisit>> GetByEmployeeAsync(int employeeId);
        Task<TVisit?> CreateAsync(TCreateVisit request);
        Task<TVisit?> UpdateAsync(int id, TUpdateVisit request);
        Task<bool> DeleteAsync(int id);
    }

    public interface IVisitService : IVisitService<VisitDto, CreateVisitRequest, UpdateVisitRequest>;
}
