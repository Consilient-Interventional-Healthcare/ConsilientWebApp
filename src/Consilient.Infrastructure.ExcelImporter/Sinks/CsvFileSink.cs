using Consilient.Infrastructure.ExcelImporter.Contracts;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace Consilient.Infrastructure.ExcelImporter.Sinks
{
    public class CsvFileSink(string filePath, CsvConfiguration? config = null) : IDataSink
    {
        private readonly string _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        private readonly CsvConfiguration _config = config ?? new CsvConfiguration(CultureInfo.InvariantCulture);
        private StreamWriter? _writer;
        private CsvWriter? _csv;
        private bool _headerWritten;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _writer = new StreamWriter(_filePath, false, new UTF8Encoding(false));
            _csv = new CsvWriter(_writer, _config);
            _headerWritten = false;

            return Task.CompletedTask;
        }

        public async Task WriteBatchAsync<TRow>(Guid batchId, IReadOnlyList<TRow> batch, CancellationToken cancellationToken = default)
            where TRow : class
        {
            if (_csv == null)
            {
                throw new InvalidOperationException("Sink not initialized");
            }

            foreach (var row in batch)
            {
                if (!_headerWritten)
                {
                    _csv.WriteHeader<TRow>();
                    await _csv.NextRecordAsync();
                    _headerWritten = true;
                }

                _csv.WriteRecord(row);
                await _csv.NextRecordAsync();
            }
        }

        public async Task FinalizeAsync(CancellationToken cancellationToken = default)
        {
            if (_csv != null)
            {
                await _csv.FlushAsync();
                _csv.Dispose();
                _csv = null;
                _writer = null; // CsvWriter disposes the underlying StreamWriter
            }
        }
    }
}
