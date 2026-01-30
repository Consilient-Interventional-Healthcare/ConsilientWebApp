using Consilient.Visits.Contracts.Models.Requests;

namespace Consilient.Visits.Contracts;

public interface IVisitServiceBillingService
{
    Task<int> CreateAsync(CreateVisitServiceBillingRequest request, CancellationToken ct = default);
    Task<int> DeleteAsync(DeleteVisitServiceBillingRequest request, CancellationToken ct = default);
}
