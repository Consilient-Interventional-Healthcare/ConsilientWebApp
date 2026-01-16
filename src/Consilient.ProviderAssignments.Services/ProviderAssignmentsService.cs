using Consilient.Background.Workers.ProviderAssignments;
using Consilient.ProviderAssignments.Contracts;
using Hangfire;

namespace Consilient.ProviderAssignments.Services
{
    public class ProviderAssignmentsService(IBackgroundJobClient backgroundJobClient) : IProviderAssignmentsService
    {
        private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

        public void Import(string fileName, int facilityId, DateOnly dateService)
        {
            _backgroundJobClient.Enqueue<ProviderAssignmentsImportWorker>(worker => worker.Import(fileName, facilityId, dateService));
        }
    }
}
