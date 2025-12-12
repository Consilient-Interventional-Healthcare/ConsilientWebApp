using Consilient.Background.Workers.Contracts;
using Consilient.DoctorAssignments.Contracts;
using Hangfire;

namespace Consilient.Background.Workers.DoctorAssignments
{
    internal class DoctorAssignmentsResolutionWorker(IDoctorAssignmentsResolver resolver) : IBackgroundWorker
    {
        public async Task ResolveAsync([FromResult] Guid batchId, CancellationToken cancellationToken = default)
        {
            await resolver.ResolveAsync(batchId, cancellationToken);
        }
    }
}
