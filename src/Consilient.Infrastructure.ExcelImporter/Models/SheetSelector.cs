namespace Consilient.Infrastructure.ExcelImporter.Models
{

    public record SheetSelector
    {
        public int? Index { get; init; }
        public string? Name { get; init; }

        public static SheetSelector FirstSheet => new() { Index = 0 };
        public static SheetSelector ByName(string name) => new() { Name = name };
        public static SheetSelector ByIndex(int index) => new() { Index = index };
    }

}