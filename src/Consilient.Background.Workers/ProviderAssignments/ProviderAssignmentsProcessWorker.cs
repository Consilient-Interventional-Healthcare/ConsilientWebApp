using Consilient.Background.Workers.Contracts;
using Consilient.ProviderAssignments.Contracts.Processing;
using System.ComponentModel;

namespace Consilient.Background.Workers.ProviderAssignments
{
    internal class ProviderAssignmentsProcessWorker(IProviderAssignmentsProcessor processor) : IBackgroundWorker
    {
        [DisplayName("Process Provider Assignments for Batch {0}")]
        public void Process(Guid batchId)
        {
            processor.ProcessAsync(batchId, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
