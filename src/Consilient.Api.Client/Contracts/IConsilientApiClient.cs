namespace Consilient.Api.Client.Contracts
{
    public interface IConsilientApiClient
    {
        IPatientsApi Patients { get; }
    }
}
