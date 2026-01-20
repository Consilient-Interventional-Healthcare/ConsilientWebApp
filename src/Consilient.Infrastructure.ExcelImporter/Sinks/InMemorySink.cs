using Consilient.Infrastructure.ExcelImporter.Contracts;

namespace Consilient.Infrastructure.ExcelImporter.Sinks
{

    public class InMemorySink<TRow> : IDataSink where TRow : class
    {
        public List<TRow> Rows { get; } = [];
        public List<ValidatedRow<TRow>> ValidatedRows { get; } = [];

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Rows.Clear();
            ValidatedRows.Clear();
            return Task.CompletedTask;
        }

        public Task WriteBatchAsync<T>(Guid batchId, IReadOnlyList<T> batch, CancellationToken cancellationToken = default)
            where T : class
        {
            // Handle ValidatedRow<TRow> from StagedExcelImporter
            if (batch is IReadOnlyList<ValidatedRow<TRow>> validatedBatch)
            {
                ValidatedRows.AddRange(validatedBatch);
                Rows.AddRange(validatedBatch.Select(vr => vr.Row));
            }
            // Handle direct TRow from original ExcelImporter
            else if (batch is IReadOnlyList<TRow> typedBatch)
            {
                Rows.AddRange(typedBatch);
            }

            return Task.CompletedTask;
        }

        public Task FinalizeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

}