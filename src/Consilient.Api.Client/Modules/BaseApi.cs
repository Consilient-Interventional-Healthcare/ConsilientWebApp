using Consilient.Api.Client.Contracts;
using Consilient.Api.Client.Models;
using Consilient.Infrastructure.Serialization;
using Newtonsoft.Json;

namespace Consilient.Api.Client.Modules
{
    internal abstract class BaseApi(HttpClient httpClient) : IApi
    {
        private readonly HttpClient _httpClient = httpClient;


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
            var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new ApiResponse<T>
            {
                IsSuccess = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                Data = JsonConvert.DeserializeObject<T>(jsonContent, JsonSerializerConfiguration.DefaultSettings)
            };
        }

        protected async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            using var request = CreateRequest(HttpMethod.Delete, url);
            return await SendAsync(request).ConfigureAwait(false);
        }

        protected async Task<HttpResponseMessage> GetAsync(string url)
        {
            using var request = CreateRequest(HttpMethod.Get, url);
            return await SendAsync(request).ConfigureAwait(false);
        }

        protected async Task<HttpResponseMessage> PostAsync(string url, object? data = default, IEnumerable<Models.File>? files = null)
        {
            using var request = CreateRequest(HttpMethod.Post, url, data, files);
            return await SendAsync(request).ConfigureAwait(false);
        }

        protected async Task<HttpResponseMessage> PutAsync(string url, object? data = default)
        {
            using var request = CreateRequest(HttpMethod.Put, url, data);
            return await SendAsync(request).ConfigureAwait(false);
        }

        private static HttpContent? CreateContent(object? data, IEnumerable<Models.File>? files = null)
        {
            var hasFiles = files != null && files.Any();
            var hasData = data != null;
            if (!hasFiles && !hasData)
            {
                return null;
            }

            if (hasData && !hasFiles)
            {
                if (data is HttpContent content)
                    return content;

                var json = JsonConvert.SerializeObject(data, JsonSerializerConfiguration.DefaultSettings);
                return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }

            var form = new MultipartFormDataContent();

            // Add data as JSON string content if present
            if (hasData)
            {
                HttpContent dataContent;
                if (data is HttpContent httpContent)
                {
                    dataContent = httpContent;
                }
                else
                {
                    var jsonString = JsonConvert.SerializeObject(data, JsonSerializerConfiguration.DefaultSettings);
                    dataContent = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
                }
                form.Add(dataContent, "data");
            }

            // Add all files
            var index = 0;
            foreach (var file in files!)
            {
                var byteContent = new ByteArrayContent(file.Content);
                byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                form.Add(byteContent, $"file_{index}", file.FileName);
                index++;
            }

            return form;
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, string url, object? data = null, IEnumerable<Models.File>? files = null)
        {
            return new HttpRequestMessage(method, url)
            {
                Content = CreateContent(data, files)
            };
        }
        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                return await _httpClient.SendAsync(request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Resolve to absolute URI if possible
                var requestUri = request.RequestUri;
                if (requestUri != null && !requestUri.IsAbsoluteUri && _httpClient.BaseAddress != null)
                {
                    requestUri = new Uri(_httpClient.BaseAddress, requestUri);
                }

                var fullUrl = requestUri?.ToString() ?? request.RequestUri?.ToString() ?? "(unknown)";
                throw new HttpRequestException($"Request to '{fullUrl}' failed.", ex);
            }
        }
    }
}
