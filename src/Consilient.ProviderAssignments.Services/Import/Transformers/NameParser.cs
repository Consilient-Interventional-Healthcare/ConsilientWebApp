using System.Text;
using System.Text.RegularExpressions;

namespace Consilient.ProviderAssignments.Services.Import.Transformers
{
    /// <summary>
    /// Static helper for parsing and normalizing names from Excel imports.
    /// </summary>
    internal static partial class NameParser
    {
        #region Patient Name Parsing

        /// <summary>
        /// Splits a full name into last and first name components.
        /// Supports "Last, First" and "First Last" formats.
        /// </summary>
        public static (string lastName, string firstName) SplitPatientName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (string.Empty, string.Empty);

            // Try "Last, First" format first
            var parts = fullName.Split(',', 2);
            if (parts.Length == 2)
            {
                var lastName = parts[0].Trim();
                var firstName = parts[1].Trim();
                return (lastName, firstName);
            }

            // Try "First Last" format
            var names = fullName.Split(' ', 2);
            if (names.Length == 2)
            {
                var firstName = names[0].Trim();
                var lastName = names[1].Trim();
                return (lastName, firstName);
            }

            // Single name, assume it's the last name
            return (fullName.Trim(), string.Empty);
        }

        /// <summary>
        /// Normalizes name casing with support for special name patterns.
        /// Handles: O'Brien, McDonald, MacArthur, hyphenated names, etc.
        /// </summary>
        public static string NormalizeCase(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = NormalizeNamePart(parts[i]);
            }
            return string.Join(' ', parts);
        }

        /// <summary>
        /// Normalizes a single name part (handles hyphenated names, apostrophes, Mc/Mac prefixes).
        /// </summary>
        private static string NormalizeNamePart(string part)
        {
            if (string.IsNullOrEmpty(part))
                return part;

            // Handle hyphenated names (e.g., "Smith-Jones")
            if (part.Contains('-'))
            {
                var hyphenParts = part.Split('-');
                for (int i = 0; i < hyphenParts.Length; i++)
                {
                    hyphenParts[i] = NormalizeNamePart(hyphenParts[i]);
                }
                return string.Join('-', hyphenParts);
            }

            // Handle apostrophe names (e.g., "O'Brien", "D'Angelo")
            var apostropheIndex = part.IndexOf('\'');
            if (apostropheIndex > 0 && apostropheIndex < part.Length - 1)
            {
                var before = part[..apostropheIndex];
                var after = part[(apostropheIndex + 1)..];
                return char.ToUpper(before[0]) + before[1..].ToLower() + "'" +
                       char.ToUpper(after[0]) + after[1..].ToLower();
            }

            // Handle "Mc" prefix (e.g., "McDonald", "McGregor")
            if (part.Length > 2 && part.StartsWith("Mc", StringComparison.OrdinalIgnoreCase))
            {
                return "Mc" + char.ToUpper(part[2]) + part[3..].ToLower();
            }

            // Handle "Mac" prefix (e.g., "MacArthur", "MacDowell")
            // Note: Be careful - not all "Mac" names follow this pattern (e.g., "Mack", "Macey")
            // Only apply if there's a capital letter after "Mac" in the original or length > 4
            if (part.Length > 4 && part.StartsWith("Mac", StringComparison.OrdinalIgnoreCase) &&
                !part.StartsWith("Mack", StringComparison.OrdinalIgnoreCase) &&
                !part.StartsWith("Mace", StringComparison.OrdinalIgnoreCase) &&
                !part.StartsWith("Macy", StringComparison.OrdinalIgnoreCase))
            {
                return "Mac" + char.ToUpper(part[3]) + part[4..].ToLower();
            }

            // Standard title case
            return char.ToUpper(part[0]) + part[1..].ToLower();
        }

        #endregion

        #region Provider Name Extraction

        /// <summary>
        /// Extracts the provider last name from a provider field.
        /// Handles multiple formats:
        /// - "Dr Smith", "Dr. Smith", "DR SMITH"
        /// - "NP Jones", "NP. Jones"
        /// - "Doctor Smith"
        /// - "Smith, MD", "Smith MD", "John Smith MD"
        /// - Plain "Smith" (fallback)
        /// </summary>
        public static string? ExtractProviderLastName(string? providerField)
        {
            if (string.IsNullOrWhiteSpace(providerField))
                return null;

            var trimmed = providerField.Trim();

            // Try prefix patterns first: "Dr. Smith", "NP Jones", "Doctor Smith"
            var prefixMatch = ProviderPrefixRegex().Match(trimmed);
            if (prefixMatch.Success)
            {
                return NormalizeCase(prefixMatch.Groups[1].Value);
            }

            // Try suffix patterns: "Smith, MD", "Smith MD", "John Smith MD"
            var suffixMatch = ProviderSuffixRegex().Match(trimmed);
            if (suffixMatch.Success)
            {
                // Group 1 is the name part before the suffix
                var namePart = suffixMatch.Groups[1].Value.Trim();
                // If it contains a space, take the last word as last name
                var lastSpace = namePart.LastIndexOf(' ');
                var lastName = lastSpace >= 0 ? namePart[(lastSpace + 1)..] : namePart;
                // Remove trailing comma if present
                lastName = lastName.TrimEnd(',').Trim();
                return NormalizeCase(lastName);
            }

            // Fallback: if it's a single word, treat it as the last name
            if (!trimmed.Contains(' '))
            {
                return NormalizeCase(trimmed);
            }

            // Multiple words without recognized pattern - take the last word
            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return NormalizeCase(parts[^1]);
            }

            return null;
        }

        // Matches prefix patterns: "Dr Smith", "Dr. Smith", "NP Jones", "Doctor Smith"
        [GeneratedRegex(@"^(?:Dr\.?|NP\.?|Doctor)\s+(\w+)", RegexOptions.IgnoreCase)]
        private static partial Regex ProviderPrefixRegex();

        // Matches suffix patterns: "Smith, MD", "Smith MD", "John Smith, MD", "Jones DO"
        [GeneratedRegex(@"^(.+?),?\s*(?:MD|DO|NP|PA|APRN|CNP|FNP|DNP)\.?\s*$", RegexOptions.IgnoreCase)]
        private static partial Regex ProviderSuffixRegex();

        #endregion

        #region Location Parsing

        /// <summary>
        /// Parses a location string like "123A" into room and bed components.
        /// </summary>
        public static (string? room, string? bed) ParseLocation(string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return (null, null);

            var match = LocationRegex().Match(location);
            return match.Success ? (match.Groups[1].Value, match.Groups[2].Value) : (null, null);
        }

        // Matches "123A" → room="123", bed="A"
        [GeneratedRegex(@"^(\d+)([A-Za-z]+)$", RegexOptions.Compiled)]
        private static partial Regex LocationRegex();

        #endregion
    }
}
