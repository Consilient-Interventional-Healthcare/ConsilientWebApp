using Consilient.Api.Client.Models;
using Consilient.Visits.Contracts.Dtos;
using Consilient.Visits.Contracts.Requests;
using Consilient.Visits.Contracts.Results;

namespace Consilient.Api.Client.Contracts
{
    public interface IVisitsStagingApi : IVisitsApi<VisitStagingDto, CreateVisitStagingRequest, UpdateVisitStagingRequest>
    {
        Task<ApiResponse<int>> PushApprovedVisitsAsync();
        Task<ApiResponse<UploadAssignmentResult>> UploadSpreadsheetAsync(Models.File file);
    }
}
