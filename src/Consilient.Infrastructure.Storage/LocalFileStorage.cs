using Consilient.Infrastructure.Storage.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Consilient.Infrastructure.Storage
{
    /// <summary>
    /// Local filesystem implementation of <see cref="IFileStorage"/>.
    /// </summary>
    public class LocalFileStorage : IFileStorage
    {
        private readonly string _basePath;
        private readonly ILogger<LocalFileStorage> _logger;

        public LocalFileStorage(IOptions<FileStorageOptions> options, ILogger<LocalFileStorage> logger)
        {
            _logger = logger;
            var opts = options.Value;
            _basePath = string.IsNullOrWhiteSpace(opts.LocalPath)
                ? Path.Combine(Path.GetTempPath(), "consilient-uploads")
                : opts.LocalPath;

            _logger.LogDebug("LocalFileStorage initialized with BasePath: {BasePath}", _basePath);

            Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
        {
            var fileReference = PathHelper.GenerateFileReference(fileName);
            var fullPath = GetFullPath(fileReference);

            _logger.LogDebug("Saving file: {FileName}, Reference: {FileReference}, Path: {FullPath}, " +
                "StreamPosition: {Position}, StreamLength: {Length}, CanSeek: {CanSeek}",
                fileName, fileReference, fullPath,
                content.CanSeek ? content.Position : -1,
                content.CanSeek ? content.Length : -1,
                content.CanSeek);

            // Reset stream position if possible
            if (content.CanSeek && content.Position != 0)
            {
                _logger.LogDebug("Resetting stream position from {Position} to 0", content.Position);
                content.Position = 0;
            }

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

            _logger.LogDebug("File saved successfully: {FileReference}, BytesWritten: {BytesWritten}",
                fileReference, fileStream.Length);

            return fileReference;
        }

        public Task<Stream> GetAsync(string fileReference, CancellationToken cancellationToken = default)
        {
            var fullPath = GetFullPath(fileReference);
            var exists = File.Exists(fullPath);

            _logger.LogDebug("Getting file: {FileReference}, Path: {FullPath}, Exists: {Exists}",
                fileReference, fullPath, exists);

            if (!exists)
            {
                throw new FileNotFoundException($"File not found: {fullPath}", fullPath);
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
            var exists = File.Exists(fullPath);

            _logger.LogDebug("Deleting file: {FileReference}, Path: {FullPath}, Exists: {Exists}",
                fileReference, fullPath, exists);

            if (exists)
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
            var exists = File.Exists(fullPath);

            _logger.LogDebug("Checking file exists: {FileReference}, Path: {FullPath}, Exists: {Exists}",
                fileReference, fullPath, exists);

            return Task.FromResult(exists);
        }

        private string GetFullPath(string fileReference)
        {
            var fullPath = PathHelper.CombineAndNormalize(_basePath, fileReference);
            _logger.LogDebug("Path resolution: Reference={FileReference}, FullPath={FullPath}",
                fileReference, fullPath);
            return fullPath;
        }
    }
}
