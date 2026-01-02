using System.Data;
using System.Reflection;

namespace Consilient.Infrastructure.ExcelImporter.Helpers
{
    public static class DataTableBuilder
    {
        public static DataTable BuildFrom<TData>(IEnumerable<TData> items, IReadOnlyList<string>? columnOrder = null) where TData : class
        {
            var table = new DataTable();

            var props = typeof(TData)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            var columns = columnOrder != null && columnOrder.Count > 0
                ? columnOrder
                : props.Keys.OrderBy(n => n).ToList();

            foreach (var colName in columns)
            {
                if (props.TryGetValue(colName, out var prop))
                {
                    var colType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    table.Columns.Add(colName, colType);
                }
                else
                {
                    // unknown property -> treat as string
                    table.Columns.Add(colName, typeof(string));
                }
            }

            foreach (var item in items)
            {
                var row = table.NewRow();
                foreach (DataColumn col in table.Columns)
                {
                    if (props.TryGetValue(col.ColumnName, out var prop))
                    {
                        var value = prop.GetValue(item) ?? DBNull.Value;
                        row[col.ColumnName] = value;
                    }
                    else
                    {
                        row[col.ColumnName] = DBNull.Value;
                    }
                }
                table.Rows.Add(row);
            }

            return table;
        }
    }
}