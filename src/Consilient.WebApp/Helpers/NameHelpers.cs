namespace Consilient.WebApp.Helpers
{
    public static class NameHelpers
    {
        public static string GetFullName(string? firstName, string? lastName)
        {
            var fullName = string.Empty;
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                fullName += firstName;
            }
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                if (fullName.Length > 0)
                {
                    fullName += " ";
                }
                fullName += lastName;
            }
            return fullName;
        }
    }
}
