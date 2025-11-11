namespace Consilient.Infrastructure.ExcelImporter.Helpers
{
    public static class ImportUtilities
    {
        public static (string FirstName, string LastName) SplitName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (string.Empty, string.Empty);
            }
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return (parts[0], string.Empty);
            }
            var firstName = parts[0];
            var lastName = string.Join(' ', parts.Skip(1));
            return (firstName, lastName);
        }
    }
}
