using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Models;

namespace Consilient.Api.Client.Modules
{
    internal class GraphQlApi(HttpClient httpClient) : BaseApi(httpClient), IGraphQlApi
    {
        public async Task<ApiResponse<GraphQlResponse>> Query(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException(nameof(query));
            }
            var resp = await PostAsync(Routes.Query(), new { query }).ConfigureAwait(false);
            return await CreateApiResponse<GraphQlResponse>(resp);
        }

        private static class Routes
        {
            private const string _base = "/graphQl";

            public static string Query() => _base;
        }
    }
}
