namespace Consilient.Api.Client.Contracts
{
    public interface IConsilientApiClient
    {
        IEmployeesApi Employees { get; }
        IFacilitiesApi Facilities { get; }
        IInsurancesApi Insurances { get; }
        IPatientsApi Patients { get; }
        IServiceTypesApi ServiceTypes { get; }
    }
}
