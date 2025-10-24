namespace Consilient.Api.Client.Contracts
{
    public interface IConsilientApiClient
    {
        IPatientsApi Patients { get; }
        IEmployeesApi Employees { get; }
        IFacilitiesApi Facilities { get; }
        IServiceTypesApi ServiceTypes { get; }

    }
}
