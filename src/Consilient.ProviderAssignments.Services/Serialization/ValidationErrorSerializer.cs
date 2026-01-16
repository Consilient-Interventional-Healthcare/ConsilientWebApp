using System.Text.Json;

namespace Consilient.ProviderAssignments.Services.Serialization
{
    /// <summary>
    /// Centralized serialization service for validation errors.
    /// Handles JSON serialization/deserialization of error lists.
    /// </summary>
    public static class ValidationErrorSerializer
    {
        /// <summary>
        /// Serializes a list of validation error messages to JSON string.
        /// Returns null if the list is empty or null.
        /// </summary>
        public static string? Serialize(List<string>? errors)
        {
            if (errors == null || errors.Count == 0)
            {
                return null;
            }

            return JsonSerializer.Serialize(errors);
        }

        /// <summary>
        /// Deserializes a JSON string to a list of validation error messages.
        /// Returns an empty list if the input is null or empty.
        /// </summary>
        public static List<string> Deserialize(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? [];
            }
            catch
            {
                // If deserialization fails, return empty list
                return [];
            }
        }
    }
}
