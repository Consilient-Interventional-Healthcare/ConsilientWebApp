namespace Consilient.Infrastructure.Storage
{
    /// <summary>
    /// Static helper class for cross-platform path operations.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Normalizes path separators to the current platform's native separator.
        /// Handles both forward slashes (/) and backslashes (\).
        /// </summary>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            return path
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Combines a base path with a file reference, normalizing separators for cross-platform compatibility.
        /// Uses Path.GetFullPath to ensure proper path resolution on all platforms.
        /// </summary>
        public static string CombineAndNormalize(string basePath, string relativePath)
        {
            var normalizedBasePath = NormalizePath(basePath);
            var normalizedRelativePath = NormalizePath(relativePath);
            var combined = Path.Combine(normalizedBasePath, normalizedRelativePath);
            // Path.GetFullPath resolves the path and normalizes separators for the current OS
            return Path.GetFullPath(combined);
        }

        /// <summary>
        /// Sanitizes a file name by removing invalid characters.
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "file";

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(sanitized) ? "file" : sanitized;
        }

        /// <summary>
        /// Generates a unique file reference with a GUID-based directory and sanitized file name.
        /// </summary>
        public static string GenerateFileReference(string fileName)
        {
            var sanitizedFileName = SanitizeFileName(fileName);
            return $"{Guid.NewGuid()}/{sanitizedFileName}";
        }
    }
}
