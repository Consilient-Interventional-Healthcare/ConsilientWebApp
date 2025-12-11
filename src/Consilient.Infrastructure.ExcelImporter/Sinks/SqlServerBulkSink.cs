using Consilient.Infrastructure.ExcelImporter.Core;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace Consilient.Infrastructure.ExcelImporter.Sinks;

public class SqlServerBulkSink : IDataSink
{
    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly List<object> _buffer = new();

    public SqlServerBulkSink(string connectionString, string tableName)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _buffer.Clear();
        return Task.CompletedTask;
    }

    public Task WriteBatchAsync<TRow>(IReadOnlyList<TRow> batch, CancellationToken cancellationToken = default)
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

        // Convert to DataTable for bulk copy
        var dataTable = BuildDataTable(_buffer);

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

    private static DataTable BuildDataTable(List<object> items)
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
        foreach (var prop in properties)
        {
            var columnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            table.Columns.Add(prop.Name, columnType);
        }

        foreach (var item in items)
        {
            var row = table.NewRow();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                row[prop.Name] = value ?? DBNull.Value;
            }
            table.Rows.Add(row);
        }

        return table;
    }
}
