namespace Consilient.Data.GraphQL
{
    public class GraphQlOptions
    {
        public const string SectionName = "GraphQl";
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int MAX_PAGE_SIZE = 50;

        public int DefaultPageSize { get; init; } = DEFAULT_PAGE_SIZE;
        public int MaxPageSize { get; init; } = MAX_PAGE_SIZE;
    }
}
