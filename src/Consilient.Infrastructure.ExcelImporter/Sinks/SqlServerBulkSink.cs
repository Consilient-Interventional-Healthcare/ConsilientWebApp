using Consilient.Infrastructure.ExcelImporter.Core;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace Consilient.Infrastructure.ExcelImporter.Sinks
{

    public class SqlServerBulkSink(
        string connectionString,
        string tableName,
        Dictionary<string, (Type Type, object Value)>? additionalColumns = null) : IDataSink
    {
        private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        private readonly string _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        private readonly List<object> _buffer = [];
        private readonly Dictionary<string, (Type Type, object Value)>? _additionalColumns = additionalColumns;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _buffer.Clear();
            return Task.CompletedTask;
        }

        public Task WriteBatchAsync<TRow>(Guid batchId, IReadOnlyList<TRow> batch, CancellationToken cancellationToken = default)
            where TRow : class
        {
            _buffer.AddRange(batch);
            return Task.CompletedTask;
        }

        public async Task FinalizeAsync(CancellationToken cancellationToken = default)
        {
            if (_buffer.Count == 0)
            {
                return;
            }

            // Convert to DataTable for bulk copy (with additional columns)
            var dataTable = BuildDataTable(_buffer, _additionalColumns);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, null)
            {
                DestinationTableName = _tableName,
                BulkCopyTimeout = 300
            };

            foreach (DataColumn column in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
        }

        private static DataTable BuildDataTable(
            List<object> items,
            Dictionary<string, (Type Type, object Value)>? additionalColumns = null)
        {
            if (items.Count == 0)
            {
                return new DataTable();
            }

            var type = items[0].GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToList();

            var table = new DataTable();

            // Add columns from object properties
            foreach (var prop in properties)
            {
                var columnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                table.Columns.Add(prop.Name, columnType);
            }

            // Add additional columns if provided
            if (additionalColumns != null)
            {
                foreach (var (columnName, (columnType, _)) in additionalColumns)
                {
                    table.Columns.Add(columnName, columnType);
                }
            }

            // Fill rows
            foreach (var item in items)
            {
                var row = table.NewRow();

                // Set property values
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    row[prop.Name] = value ?? DBNull.Value;
                }

                // Set additional column values (same for every row)
                if (additionalColumns != null)
                {
                    foreach (var (columnName, (_, columnValue)) in additionalColumns)
                    {
                        row[columnName] = columnValue ?? DBNull.Value;
                    }
                }

                table.Rows.Add(row);
            }

            return table;
        }
    }

}