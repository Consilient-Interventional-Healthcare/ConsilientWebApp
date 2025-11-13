using Consilient.Visits.Contracts.Dtos;
using Consilient.Visits.Contracts.Requests;
using Consilient.Visits.Contracts.Results;

namespace Consilient.Visits.Contracts
{
    public interface IVisitStagingService : IVisitService<VisitStagingDto, CreateVisitStagingRequest, UpdateVisitStagingRequest>
    {
        Task<int> PushApprovedVisitsAsync();
        Task<UploadAssignmentResult> UploadAssignmentAsync(MemoryStream spreadsheet);
    }
}
