using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;

namespace Consilient.Patients.Contracts
{
    public interface IStagingPatientVisitService : IPatientVisitService<StagingPatientVisitDto, CreateStagingPatientVisitRequest, UpdateStagingPatientVisitRequest>
    {
    }
}
