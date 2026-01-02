using Consilient.Api.Client.Models;
using System.Text.Json;

namespace Consilient.Api.Client
{
    public static class ApiResponseExtensions
    {
        // Cache JsonSerializerOptions instance to avoid CA1869
        private static readonly JsonSerializerOptions _cachedJsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static T? Unwrap<T>(this ApiResponse<T> response)
        {
            return response.IsSuccess ? response.Data : throw new InvalidOperationException($"API call failed with status code {response.StatusCode}: {response.ErrorMessage}");
        }

        public static T? Unwrap<T>(this GraphQlResponse response, string key)
        {
            if (response.Errors != null && response.Errors.Count > 1)
            {
                throw new InvalidOperationException($"GraphQLErrors: {string.Join("| ", response.Errors)}");
            }
            if (response.Data == null || !response.Data.TryGetValue(key, out var value))
            {
                throw new InvalidOperationException($"GraphQL response does not contain expected key '{key}'.");
            }

            if (value == null)
            {
                return default;
            }

            // If it's already the desired CLR type
            if (value is T typed)
            {
                return typed;
            }

            // Handle JsonElement (common when deserializing JSON to object)
            if (value is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Null)
                {
                    return default;
                }

                try
                {
                    // Use cached JsonSerializerOptions instance
                    return je.Deserialize<T>(_cachedJsonSerializerOptions);
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"Failed to deserialize JsonElement for GraphQL key '{key}'.", ex);
                }
            }

            // Handle JsonDocument
            if (value is JsonDocument jd)
            {
                try
                {
                    return jd.RootElement.Deserialize<T>(_cachedJsonSerializerOptions);
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"Failed to deserialize JsonDocument for GraphQL key '{key}'.", ex);
                }
            }

            // If the value is a string containing JSON
            if (value is string s)
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(s, _cachedJsonSerializerOptions);
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"Failed to deserialize JSON string for GraphQL key '{key}'.", ex);
                }
            }

            // Fallback: serialize the object back to JSON then deserialize to target type
            try
            {
                var serialized = JsonSerializer.Serialize(value, _cachedJsonSerializerOptions);
                return JsonSerializer.Deserialize<T>(serialized, _cachedJsonSerializerOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Unable to convert GraphQL value for key '{key}' to {typeof(T).FullName}.", ex);
            }
        }
    }
}
