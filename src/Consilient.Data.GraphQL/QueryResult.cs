namespace Consilient.Data.GraphQL;

public class QueryResult
{
    public List<GraphQlError>? Errors { get; set; }
    public Dictionary<string, object?>? Data { get; set; }
    public Dictionary<string, object>? Extensions { get; set; }
}
