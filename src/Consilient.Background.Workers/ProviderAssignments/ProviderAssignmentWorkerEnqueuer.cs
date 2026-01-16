using Consilient.ProviderAssignments.Contracts;
using Hangfire;

namespace Consilient.Background.Workers.ProviderAssignments
{
    public class ProviderAssignmentsImportWorkerEnqueuer(IBackgroundJobClient backgroundJobClient)
    {
        public FileUploadResult Import(string fileName, int facilityId, DateOnly dateService, int enqueuedByUserId)
        {
            var batchId = Guid.NewGuid();

            // Enqueue the import job
            var parentJobId = backgroundJobClient.Enqueue<ProviderAssignmentsImportWorker>(worker => worker.Import(fileName, facilityId, dateService, enqueuedByUserId, batchId, null!));

            // Chain the resolution job to run after import completes, passing the BatchId from the import result
            backgroundJobClient.ContinueJobWith<ProviderAssignmentsResolutionWorker>(parentJobId, x => x.Resolve(default), JobContinuationOptions.OnlyOnSucceededState);

            return new FileUploadResult
            {
                FileName = Path.GetFileName(fileName),

                ServiceDate = dateService,
                FacilityId = facilityId,
                Message = "File uploaded successfully and queued for processing.",
                BatchId = batchId
            };
        }
    }
}
