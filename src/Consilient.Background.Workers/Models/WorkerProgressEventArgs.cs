namespace Consilient.Background.Workers.Models
{
    /// <summary>
    /// Generic progress event args for background workers
    /// </summary>
    public class WorkerProgressEventArgs : EventArgs
    {
        public string JobId { get; init; } = string.Empty;
        public string Stage { get; init; } = string.Empty;
        public int TotalItems { get; init; }
        public int ProcessedItems { get; init; }
        public int PercentComplete => TotalItems > 0 ? (int)((double)ProcessedItems / TotalItems * 100) : 0;
        public string? CurrentOperation { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object>? AdditionalData { get; init; }
    }
}