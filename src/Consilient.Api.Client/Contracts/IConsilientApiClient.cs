namespace Consilient.Api.Client.Contracts
{
    public interface IConsilientApiClient
    {
        IEmployeesApi Employees { get; }
        IFacilitiesApi Facilities { get; }
        IGraphQlApi GraphQl { get; }
        IInsurancesApi Insurances { get; }
        IPatientsApi Patients { get; }
        IPatientVisitsApi PatientVisits { get; }
        IServiceTypesApi ServiceTypes { get; }
        IStagingPatientVisitsApi StagingPatientVisits { get; }
    }
}
