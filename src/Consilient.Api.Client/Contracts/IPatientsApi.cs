using Consilient.Patients.Contracts.Dtos;

namespace Consilient.Api.Client.Contracts
{
    public interface IPatientsApi
    {
        public Task<PatientDto?> GetByMrnAsync(int mrn);
    }
}
