using Consilient.Api.Client.Contracts;

namespace Consilient.Api.Client
{
    public abstract class BaseApi(HttpClient httpClient) : IApi
    {
        protected HttpClient HttpClient { get; } = httpClient;
    }
}
