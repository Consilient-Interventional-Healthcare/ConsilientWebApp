using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Models;

namespace Consilient.Api.Client.Modules
{
    internal class GraphQlApi(HttpClient httpClient) : BaseApi(httpClient), IGraphQlApi
    {
        public Task<ApiResponse<IEnumerable<TData>>> Query<TData>(string query)
        {
            throw new NotImplementedException();
        }
    }
}
