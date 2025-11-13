namespace Consilient.Api.Helpers
{
    public class FileValidationResult
    {
        public bool IsValid { get; init; }
        public string? ErrorMessage { get; init; }

        public static FileValidationResult Success() => new() { IsValid = true };
        
        public static FileValidationResult Failure(string errorMessage) => 
            new() { IsValid = false, ErrorMessage = errorMessage };
    }
}