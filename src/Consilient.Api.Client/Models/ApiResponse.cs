namespace Consilient.Api.Client.Models
{
    public class ApiResponse<T>
    {
        public T? Data { get; init; }
        public bool IsSuccess { get; init; }
        public int StatusCode { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
