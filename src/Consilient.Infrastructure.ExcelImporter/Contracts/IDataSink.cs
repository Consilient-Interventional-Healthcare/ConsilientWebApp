namespace Consilient.Infrastructure.ExcelImporter.Contracts;

public interface IDataSink
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task WriteBatchAsync<TRow>(Guid batchId, IReadOnlyList<TRow> batch, CancellationToken cancellationToken = default)
        where TRow : class;

    Task FinalizeAsync(CancellationToken cancellationToken = default);
}
