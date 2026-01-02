using Consilient.Api.Client.Models;

namespace Consilient.Api.Client.Contracts
{
    public interface IGraphQlApi : IApi
    {
        Task<ApiResponse<GraphQlResponse>> Query(string query);
    }
}
