using Consilient.Infrastructure.Storage.Contracts;
using Microsoft.Extensions.Options;

namespace Consilient.Infrastructure.Storage
{
    /// <summary>
    /// Local filesystem implementation of <see cref="IFileStorage"/>.
    /// </summary>
    public class LocalFileStorage : IFileStorage
    {
        private readonly string _basePath;

        public LocalFileStorage(IOptions<FileStorageOptions> options)
        {
            var opts = options.Value;
            _basePath = string.IsNullOrWhiteSpace(opts.LocalPath)
                ? Path.Combine(Path.GetTempPath(), "consilient-uploads")
                : opts.LocalPath;

            Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
        {
            var fileReference = GenerateFileReference(fileName);
            var fullPath = GetFullPath(fileReference);

            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var fileStream = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            await content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

            return fileReference;
        }

        public Task<Stream> GetAsync(string fileReference, CancellationToken cancellationToken = default)
        {
            var fullPath = GetFullPath(fileReference);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {fileReference}", fileReference);
            }

            Stream stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            return Task.FromResult(stream);
        }

        public Task DeleteAsync(string fileReference, CancellationToken cancellationToken = default)
        {
            var fullPath = GetFullPath(fileReference);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);

                // Try to remove the parent directory if empty
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(directory).Any())
                        {
                            Directory.Delete(directory);
                        }
                    }
                    catch
                    {
                        // Ignore errors when cleaning up empty directories
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string fileReference, CancellationToken cancellationToken = default)
        {
            var fullPath = GetFullPath(fileReference);
            return Task.FromResult(File.Exists(fullPath));
        }

        private static string GenerateFileReference(string fileName)
        {
            var sanitizedFileName = SanitizeFileName(fileName);
            return $"{Guid.NewGuid()}/{sanitizedFileName}";
        }

        private string GetFullPath(string fileReference)
        {
            return Path.Combine(_basePath, fileReference);
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(sanitized) ? "file" : sanitized;
        }
    }
}
