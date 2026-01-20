namespace Consilient.Infrastructure.ExcelImporter.Contracts
{

    public record ColumnMapping
    {
        public required Dictionary<string, string> HeaderToPropertyMap { get; init; }
        public HashSet<string> RequiredColumns { get; init; } = new();
        public StringComparison ComparisonType { get; init; } = StringComparison.OrdinalIgnoreCase;

        public static ColumnMappingBuilder Builder() => new();
    }

    public class ColumnMappingBuilder
    {
        private readonly Dictionary<string, string> _map = new();
        private readonly HashSet<string> _required = new();

        public ColumnMappingBuilder Map(string header, string property)
        {
            _map[header] = property;
            return this;
        }

        public ColumnMappingBuilder MapRequired(string header, string property)
        {
            _map[header] = property;
            _required.Add(header);
            return this;
        }

        public ColumnMapping Build() => new()
        {
            HeaderToPropertyMap = _map,
            RequiredColumns = _required
        };
    }

}
