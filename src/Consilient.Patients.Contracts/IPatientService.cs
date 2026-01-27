using Consilient.Patients.Contracts.Models;
using Consilient.Patients.Contracts.Models.Requests;

namespace Consilient.Patients.Contracts;

public interface IPatientService
{
    Task<PatientDto?> GetByMrnAsync(int mrn);
    Task<PatientDto?> CreateAsync(CreatePatientRequest request);
}
