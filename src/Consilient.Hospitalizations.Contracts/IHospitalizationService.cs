using Consilient.Hospitalizations.Contracts.Models;

namespace Consilient.Hospitalizations.Contracts
{
    public interface IHospitalizationService
    {
        Task<HospitalizationDto?> GetHospitalizationById(int id);
        Task<IEnumerable<HospitalizationStatusDto>> GetHospitalizationStatuses();
    }
}
