using Consilient.Infrastructure.ExcelImporter.Contracts;
using System.Reflection;

namespace Consilient.Infrastructure.ExcelImporter.Transformers;


public class TrimStringsTransformer<TRow> : IRowTransformer<TRow> where TRow : class
{
    public TRow Transform(TRow row)
    {
        var properties = typeof(TRow)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.CanWrite);

        foreach (var prop in properties)
        {
            if (prop.GetValue(row) is string value && !string.IsNullOrEmpty(value))
            {
                prop.SetValue(row, value.Trim());
            }
        }

        return row;
    }
}