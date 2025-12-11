using System.Globalization;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Helpers
{
    /// <summary>
    /// Helper utilities for asserting CSV content in tests.
    /// </summary>
    public static class CsvTestHelper
    {
        /// <summary>
        /// Reads the CSV at <paramref name="csvPath"/>, parses the header and first data row,
        /// and compares the first data row fields to the provided <paramref name="expected"/>
        /// key/value pairs where the key is the column name.
        /// </summary>
        /// <param name="csvPath">Path to the CSV file.</param>
        /// <param name="expected">Dictionary of expected column-name -> value pairs to compare.</param>
        /// <param name="differences">If false is returned, contains human readable difference messages.</param>
        /// <param name="separator">CSV separator character (defaults to ',').</param>
        /// <param name="ignoreCase">If true, column name lookup is case-insensitive.</param>
        /// <returns>True when all expected key/value pairs match the first data row; otherwise false.</returns>
        public static bool CompareFirstRowToExpected(
            string csvPath,
            IDictionary<string, object?> expected,
            out List<string> differences,
            char separator = ',',
            bool ignoreCase = false)
        {
            differences = [];

            if (string.IsNullOrWhiteSpace(csvPath))
            {
                differences.Add("csvPath was null or empty.");
                return false;
            }

            if (!File.Exists(csvPath))
            {
                differences.Add($"CSV file not found: {csvPath}");
                return false;
            }

            if (expected == null || expected.Count == 0)
            {
                // nothing to compare -> consider it a match
                return true;
            }

            using var reader = new StreamReader(csvPath);
            string? headerLine = null;
            string? firstDataLine = null;

            // read header
            while (!reader.EndOfStream && string.IsNullOrWhiteSpace(headerLine))
            {
                headerLine = reader.ReadLine();
            }

            if (headerLine == null)
            {
                differences.Add("CSV contains no header line.");
                return false;
            }

            // read first non-empty data line
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    firstDataLine = line;
                    break;
                }
            }

            if (firstDataLine == null)
            {
                differences.Add("CSV contains header but no data rows.");
                return false;
            }

            var headers = ParseCsvLine(headerLine, separator);
            var fields = ParseCsvLine(firstDataLine, separator);

            // build header lookup
            var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            var headerToIndex = new Dictionary<string, int>(comparer);
            for (var i = 0; i < headers.Count; i++)
            {
                var name = headers[i] ?? string.Empty;
                if (!headerToIndex.ContainsKey(name))
                {
                    headerToIndex[name] = i;
                }
            }

            foreach (var kvp in expected)
            {
                var columnName = kvp.Key ?? string.Empty;
                if (!headerToIndex.TryGetValue(columnName, out var index))
                {
                    differences.Add($"Missing column: '{columnName}'");
                    continue;
                }

                var actual = index < fields.Count ? fields[index] : string.Empty;
                var expectedStr = ConvertValueToCsvField(kvp.Value);

                if (!string.Equals(actual, expectedStr, StringComparison.Ordinal))
                {
                    differences.Add($"Column '{columnName}': expected '{expectedStr}' but found '{actual}'");
                }
            }

            return differences.Count == 0;
        }

        private static string ConvertValueToCsvField(object? value)
        {
            if (value == null || value is DBNull)
            {
                return string.Empty;
            }

            return value switch
            {
                DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
                DateOnly d => d.ToString("O", CultureInfo.InvariantCulture),
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };
        }

        // Simple CSV line parser that handles quoted fields and escaped quotes ("") per RFC-style CSV.
        private static List<string> ParseCsvLine(string line, char separator)
        {
            var result = new List<string>();
            if (line == null)
            {
                return result;
            }

            var current = new System.Text.StringBuilder();
            var inQuotes = false;
            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // lookahead for escaped quote
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++; // skip next quote
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == separator)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            result.Add(current.ToString());
            return result;
        }
    }
}