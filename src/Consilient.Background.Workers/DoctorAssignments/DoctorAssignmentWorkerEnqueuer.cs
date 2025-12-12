using Hangfire;

namespace Consilient.Background.Workers.DoctorAssignments
{
    public class DoctorAssignmentsImportWorkerEnqueuer(IBackgroundJobClient backgroundJobClient)
    {
        public void Import(string fileName, int facilityId, DateOnly dateService)
        {
            // Enqueue the import job
            var parentJobId = backgroundJobClient.Enqueue<DoctorAssignmentsImportWorker>(worker => worker.Import(fileName, facilityId, dateService, null!));

            // Chain the resolution job to run after import completes, passing the BatchId from the import result
            backgroundJobClient.ContinueJobWith<DoctorAssignmentsResolutionWorker>(parentJobId, x => x.ResolveAsync(default, CancellationToken.None).Wait(), JobContinuationOptions.OnlyOnSucceededState);
        }
    }
}
