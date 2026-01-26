using Consilient.Api.Hubs;
using Consilient.Background.Workers.Contracts;
using Consilient.Background.Workers.Models;
using Consilient.Background.Workers.ProviderAssignments;
using Microsoft.AspNetCore.SignalR;

namespace Consilient.Api.Infra.SignalR
{
    public class WorkerProgressListener(IHubContext<ProgressHub> hubContext)
    {
        private readonly IHubContext<ProgressHub> _hubContext = hubContext;

        public void SubscribeToWorker(IBackgroundWorker worker)
        {
            if (worker is ProviderAssignmentsImportWorker importWorker)
            {
                importWorker.ProgressChanged += OnProgressChanged;
            }
        }

        private async void OnProgressChanged(object? sender, WorkerProgressEventArgs e)
        {
            await _hubContext.Clients
                .Group(e.JobId)
                .SendAsync("JobProgress", new
                {
                    jobId = e.JobId,
                    stage = e.Stage,
                    totalItems = e.TotalItems,
                    processedItems = e.ProcessedItems,
                    percentComplete = e.PercentComplete,
                    currentOperation = e.CurrentOperation,
                    timestamp = e.Timestamp,
                    additionalData = e.AdditionalData
                });
        }
    }
}