using Consilient.Infrastructure.ExcelImporter.Core;

namespace Consilient.Infrastructure.ExcelImporter.Sinks
{

    public class InMemorySink<TRow> : IDataSink where TRow : class
    {
        public List<TRow> Rows { get; } = [];

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Rows.Clear();
            return Task.CompletedTask;
        }

        public Task<Guid?> WriteBatchAsync<T>(IReadOnlyList<T> batch, CancellationToken cancellationToken = default)
            where T : class
        {
            if (batch is IReadOnlyList<TRow> typedBatch)
            {
                Rows.AddRange(typedBatch);
            }

            return Task.FromResult<Guid?>(Guid.NewGuid());
        }

        public Task FinalizeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

}