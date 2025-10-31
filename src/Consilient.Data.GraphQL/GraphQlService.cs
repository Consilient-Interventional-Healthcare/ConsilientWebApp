using Consilient.Data.GraphQL.Contracts;
using EntityGraphQL.Schema;
using Microsoft.Extensions.Logging;

namespace Consilient.Data.GraphQL
{
    internal class GraphQlService(ConsilientDbContext dbContext, SchemaProvider<ConsilientDbContext> schemaProvider, ILogger<GraphQlService> logger) : IGraphQlService
    {
        public async Task<QueryResult> ExecuteGraphQl(QueryRequest request, IServiceProvider sp)
        {
            var results = await schemaProvider.ExecuteRequestWithContextAsync(new EntityGraphQL.QueryRequest { Query = request.Query }, dbContext, sp, null);
            if (results.Errors != null && results.Errors.Count != 0)
            {
                foreach (var error in results.Errors)
                {
                    logger.LogError("{message:l}", error.Message);
                }
            }
            return MapToQueryResult(results);
        }

        private static QueryResult MapToQueryResult(EntityGraphQL.QueryResult results)
        {
            var i = new QueryResult
            {
                Errors = results.Errors?.Select(MapGraphQlError).ToList(),
                Data = results.Data,
                Extensions = results.Extensions
            };
            return i;
        }

        private static GraphQlError MapGraphQlError(EntityGraphQL.GraphQLError error)
        {
            return new GraphQlError
            {
                Message = error.Message
            };
        }
    }

}
