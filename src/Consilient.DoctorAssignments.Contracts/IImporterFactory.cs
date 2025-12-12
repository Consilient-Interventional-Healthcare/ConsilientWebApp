namespace Consilient.DoctorAssignments.Contracts
{
    public interface IImporterFactory
    {
        IDoctorAssignmentsImporter Create(int facilityId, DateOnly serviceDate);
    }
}
