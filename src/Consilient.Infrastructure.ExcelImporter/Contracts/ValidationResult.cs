namespace Consilient.Infrastructure.ExcelImporter.Contracts
{

    public record ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<ValidationError> Errors { get; init; } = new();

        public static ValidationResult Success() => new();
        public static ValidationResult Failed(params ValidationError[] errors) =>
            new() { Errors = errors.ToList() };
    }

}
