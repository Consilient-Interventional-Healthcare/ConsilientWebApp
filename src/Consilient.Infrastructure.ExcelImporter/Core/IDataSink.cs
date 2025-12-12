namespace Consilient.Infrastructure.ExcelImporter.Core
{
    public interface IDataSink
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);

        Task<Guid?> WriteBatchAsync<TRow>(IReadOnlyList<TRow> batch, CancellationToken cancellationToken = default)
            where TRow : class;

        Task FinalizeAsync(CancellationToken cancellationToken = default);
    }
}
