namespace Consilient.Api.Client
{
    public static class ApiResponseExtensions
    {
        public static T? Unwrap<T>(this ApiResponse<T> response)
        {
            if (!response.IsSuccess)
            {
                throw new InvalidOperationException($"API call failed with status code {response.StatusCode}: {response.ErrorMessage}");
            }
            return response.Data;
        }
    }
}
