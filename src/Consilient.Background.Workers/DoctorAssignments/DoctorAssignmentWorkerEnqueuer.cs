using Consilient.DoctorAssignments.Contracts;
using Hangfire;

namespace Consilient.Background.Workers.DoctorAssignments
{
    public class DoctorAssignmentsImportWorkerEnqueuer(IBackgroundJobClient backgroundJobClient)
    {
        public FileUploadResult Import(string fileName, int facilityId, DateOnly dateService, int enqueuedByUserId)
        {
            // Enqueue the import job
            var parentJobId = backgroundJobClient.Enqueue<DoctorAssignmentsImportWorker>(worker => worker.Import(fileName, facilityId, dateService, enqueuedByUserId, null!));

            // Chain the resolution job to run after import completes, passing the BatchId from the import result
            backgroundJobClient.ContinueJobWith<DoctorAssignmentsResolutionWorker>(parentJobId, x => x.Resolve(default), JobContinuationOptions.OnlyOnSucceededState);

            return new FileUploadResult
            {
                FileName = Path.GetFileName(fileName),
                ServiceDate = dateService,
                FacilityId = facilityId,
                Message = "File uploaded successfully and queued for processing."
            };
        }
    }
}
