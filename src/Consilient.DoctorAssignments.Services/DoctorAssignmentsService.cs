using Hangfire;

namespace Consilient.DoctorAssignments.Services
{
    public class DoctorAssignmentsService(IBackgroundJobClient backgroundJobClient) : IDoctorAssignmentsService
    {
        private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

        public void Import(string fileName, int facilityId, DateOnly dateService)
        {
            _backgroundJobClient.Enqueue<ImportDoctorAssignmentsWorker>(worker => worker.Import(fileName, facilityId, dateService));
        }
    }
}
