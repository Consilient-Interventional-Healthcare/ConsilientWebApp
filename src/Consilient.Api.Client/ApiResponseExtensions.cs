using Consilient.Api.Client.Models;

namespace Consilient.Api.Client
{
    public static class ApiResponseExtensions
    {
        public static T? Unwrap<T>(this ApiResponse<T> response)
        {
            return response.IsSuccess ? response.Data : throw new InvalidOperationException($"API call failed with status code {response.StatusCode}: {response.ErrorMessage}");
        }
    }
}
