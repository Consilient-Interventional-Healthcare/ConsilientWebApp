using Consilient.Api.Client.Models;

namespace Consilient.Api.Client.Contracts
{
    public interface IGraphQlApi : IApi
    {
        Task<ApiResponse<IEnumerable<TData>>> Query<TData>(string query);
    }
}
