using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Modules;

namespace Consilient.Api.Client
{
    internal class ConsilientApiClient : IConsilientApiClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool _disposed;

        public ConsilientApiClient(Func<HttpClient> httpClientFactory)
        {
            _httpClient = httpClientFactory.Invoke();
            Employees = new EmployeesApi(_httpClient);
            Facilities = new FacilitiesApi(_httpClient);
            GraphQl = new GraphQlApi(_httpClient);
            Insurances = new InsurancesApi(_httpClient);
            Patients = new PatientsApi(_httpClient);
            Visits = new VisitsApi(_httpClient);
            ServiceTypes = new ServiceTypesApi(_httpClient);
        }

        ~ConsilientApiClient()
        {
            Dispose(disposing: false);
        }

        public IEmployeesApi Employees { get; }
        public IFacilitiesApi Facilities { get; }
        public IGraphQlApi GraphQl { get; }
        public IInsurancesApi Insurances { get; }
        public IPatientsApi Patients { get; }
        public IServiceTypesApi ServiceTypes { get; }
        public IVisitsApi Visits { get; }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _httpClient.Dispose();
            }

            _disposed = true;
        }
    }
}
