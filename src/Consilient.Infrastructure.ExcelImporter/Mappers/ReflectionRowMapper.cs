using Consilient.Infrastructure.ExcelImporter.Contracts;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Reflection;

namespace Consilient.Infrastructure.ExcelImporter.Mappers
{

    public class ReflectionRowMapper<TRow> : IRowMapper<TRow> where TRow : class, new()
    {
        private readonly Dictionary<string, PropertyInfo> _propertyCache = [];
        private readonly ILogger<ReflectionRowMapper<TRow>> _logger;

        public ReflectionRowMapper(ILogger<ReflectionRowMapper<TRow>> logger)
        {
            _logger = logger;
            CacheProperties();
        }

        private void CacheProperties()
        {
            var properties = typeof(TRow)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite);

            foreach (var prop in properties)
            {
                _propertyCache[prop.Name] = prop;
            }
        }

        public Result<TRow> Map(ExcelRow excelRow, ColumnMapping mapping)
        {
            var instance = new TRow();

            foreach (var (header, propertyName) in mapping.HeaderToPropertyMap)
            {
                if (!excelRow.Cells.TryGetValue(header, out var cellValue))
                {
                    if (mapping.RequiredColumns.Contains(header))
                    {
                        return Result<TRow>.Failure($"Required column '{header}' not found in row {excelRow.RowNumber}");
                    }

                    continue;
                }

                if (!_propertyCache.TryGetValue(propertyName, out var property))
                {
                    _logger.LogWarning("Property {PropertyName} not found on {TypeName}", propertyName, typeof(TRow).Name);
                    continue;
                }

                try
                {
                    var convertedValue = ConvertValue(cellValue, property.PropertyType);
                    property.SetValue(instance, convertedValue);
                }
                catch (Exception ex)
                {
                    return Result<TRow>.Failure(
                        $"Failed to convert '{cellValue}' to {property.PropertyType.Name} for property {propertyName} at row {excelRow.RowNumber}: {ex.Message}");
                }
            }

            return Result<TRow>.Success(instance);
        }

        private static object? ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                {
                    return Activator.CreateInstance(targetType);
                }

                return null;
            }

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType == typeof(string))
            {
                return value;
            }

            if (underlyingType == typeof(DateTime))
            {
                return DateTime.Parse(value, CultureInfo.InvariantCulture);
            }

            if (underlyingType == typeof(DateOnly))
            {
                if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, out var dateOnly))
                    return dateOnly;

                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, out var dateTime))
                    return DateOnly.FromDateTime(dateTime);

                throw new FormatException($"Unable to parse '{value}' as DateOnly");
            }

            if (underlyingType == typeof(TimeOnly))
            {
                return TimeOnly.Parse(value, CultureInfo.InvariantCulture);
            }

            if (underlyingType == typeof(bool))
            {
                return ParseBoolean(value);
            }

            if (underlyingType.IsEnum)
            {
                return Enum.Parse(underlyingType, value, ignoreCase: true);
            }

            return Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
        }

        private static bool ParseBoolean(string value)
        {
            var normalized = value.Trim().ToLowerInvariant();
            return normalized is "true" or "1" or "yes" or "y" or "x";
        }
    }

}