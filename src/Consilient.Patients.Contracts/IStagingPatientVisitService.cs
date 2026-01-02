using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using Consilient.Patients.Contracts.Results;

namespace Consilient.Patients.Contracts
{
    public interface IStagingPatientVisitService : IPatientVisitService<StagingPatientVisitDto, CreateStagingPatientVisitRequest, UpdateStagingPatientVisitRequest>
    {
        Task<int> PushApprovedPatientVisitsAsync();
        Task<UploadSpreadsheetResult> UploadSpreadsheetAsync(MemoryStream spreadsheet);
    }
}
