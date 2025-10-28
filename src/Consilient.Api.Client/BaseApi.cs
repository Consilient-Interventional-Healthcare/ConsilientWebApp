using Consilient.Api.Client.Contracts;
using Consilient.Shared.Contracts.Dtos;
using System.Net.Http.Json;

namespace Consilient.Api.Client
{
    internal abstract class BaseApi(HttpClient httpClient) : IApi
    {
        protected HttpClient HttpClient { get; } = httpClient;

        protected static async Task<ApiResponse<T>> CreateApiResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    ErrorMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                };
            }
            return new ApiResponse<T>
            {
                IsSuccess = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                Data = await response.Content.ReadFromJsonAsync<T?>().ConfigureAwait(false)
            };
        }
    }
}
