namespace Consilient.Data.GraphQL.Contracts;

public interface IGraphQlService
{
    Task<QueryResult> ExecuteGraphQl(QueryRequest request, IServiceProvider sp);
}
