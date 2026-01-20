using Consilient.Background.Workers.Models;
using Consilient.ProviderAssignments.Contracts.Import;
using Hangfire;

namespace Consilient.Background.Workers.ProviderAssignments
{
    public class ProviderAssignmentsImportWorkerEnqueuer(IBackgroundJobClient backgroundJobClient)
    {
        public FileUploadResult Import(Guid batchId, string fileName, int facilityId, DateOnly dateService, int enqueuedByUserId)
        {
            var input = new ProviderAssignmentsImportInput
            {
                FileReference = fileName,
                FacilityId = facilityId,
                ServiceDate = dateService,
                EnqueuedByUserId = enqueuedByUserId
            };

            // Enqueue the import job
            var importJobId = backgroundJobClient.Enqueue<ProviderAssignmentsImportWorker>(worker => worker.Import(batchId, input, null!));

            // Chain the resolution job to run after import completes, passing the BatchId from the import result
            backgroundJobClient.ContinueJobWith<ProviderAssignmentsResolutionWorker>(importJobId, x => x.Resolve(batchId, facilityId, dateService), JobContinuationOptions.OnlyOnSucceededState);

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
