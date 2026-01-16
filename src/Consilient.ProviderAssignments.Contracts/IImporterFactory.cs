namespace Consilient.ProviderAssignments.Contracts
{
    public interface IImporterFactory
    {
        IProviderAssignmentsImporter Create(int facilityId, DateOnly serviceDate);
    }
}
