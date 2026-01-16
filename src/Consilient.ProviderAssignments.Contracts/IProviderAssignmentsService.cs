namespace Consilient.ProviderAssignments.Contracts
{
    public interface IProviderAssignmentsService
    {
        void Import(string fileName, int facilityId, DateOnly dateService);
    }
}
