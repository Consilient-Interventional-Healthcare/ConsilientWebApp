using Consilient.Api.Client.Models;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using Consilient.Patients.Contracts.Results;

namespace Consilient.Api.Client.Contracts
{
    public interface IStagingPatientVisitsApi : IPatientVisitsApi<StagingPatientVisitDto, CreateStagingPatientVisitRequest, UpdateStagingPatientVisitRequest>
    {
        Task<ApiResponse<int>> PushApprovedPatientVisitsAsync();
        Task<ApiResponse<UploadSpreadsheetResult>> UploadSpreadsheetAsync(Models.File file);
    }
}
