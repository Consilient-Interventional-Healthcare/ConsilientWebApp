using Consilient.Data.GraphQL;
using Consilient.Data.GraphQL.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consilient.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class GraphQlController(IGraphQlService graphQlService) : ControllerBase
    {
        [HttpPost("")]
        public async Task<QueryResult> GraphQl([FromBody] QueryRequest query)
        {
            var results = await graphQlService.ExecuteGraphQl(query, HttpContext.RequestServices);
            return results;
        }
    }
}
