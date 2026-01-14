using Consilient.Background.Workers.Contracts;
using Consilient.DoctorAssignments.Contracts;
using Hangfire;

namespace Consilient.Background.Workers.DoctorAssignments
{
    internal class DoctorAssignmentsResolutionWorker(IDoctorAssignmentsResolver resolver) : IBackgroundWorker
    {
        public void Resolve([FromResult] Guid batchId)
        {
            resolver.ResolveAsync(batchId, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
