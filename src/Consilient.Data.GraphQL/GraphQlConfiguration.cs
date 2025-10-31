namespace Consilient.Data.GraphQL
{
    public class GraphQlConfiguration
    {
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int MAX_PAGE_SIZE = 50;

        public int DefaultPageSize { get; set; } = DEFAULT_PAGE_SIZE;
        public int MaxPageSize { get; set; } = MAX_PAGE_SIZE;
    }
}
