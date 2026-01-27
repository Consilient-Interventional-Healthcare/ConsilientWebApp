using Azure;
using Azure.Storage.Blobs;
using Consilient.Infrastructure.Storage.Contracts;
using Microsoft.Extensions.Options;

namespace Consilient.Infrastructure.Storage;

/// <summary>
/// Azure Blob Storage implementation of <see cref="IFileStorage"/>.
/// </summary>
public class AzureBlobFileStorage : IFileStorage
{
    private readonly BlobContainerClient _containerClient;
    private bool _containerEnsured;
    private readonly SemaphoreSlim _containerLock = new(1, 1);

    public AzureBlobFileStorage(IOptions<FileStorageOptions> options)
    {
        var opts = options.Value;

        if (string.IsNullOrWhiteSpace(opts.AzureBlobConnectionString))
        {
            throw new InvalidOperationException(
                "AzureBlobConnectionString must be configured when using AzureBlob provider.");
        }

        var blobServiceClient = new BlobServiceClient(opts.AzureBlobConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(opts.ContainerName);
    }

    public async Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        await EnsureContainerExistsAsync(cancellationToken).ConfigureAwait(false);

        var fileReference = GenerateFileReference(fileName);
        var blobClient = _containerClient.GetBlobClient(fileReference);

        await blobClient.UploadAsync(content, overwrite: true, cancellationToken).ConfigureAwait(false);

        return fileReference;
    }

    public async Task<Stream> GetAsync(string fileReference, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(fileReference);

        try
        {
            return await blobClient.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new FileNotFoundException($"File not found: {fileReference}", fileReference);
        }
    }

    public async Task DeleteAsync(string fileReference, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(fileReference);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(string fileReference, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(fileReference);
        var response = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
        return response.Value;
    }

    private async Task EnsureContainerExistsAsync(CancellationToken cancellationToken)
    {
        if (_containerEnsured) return;

        await _containerLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_containerEnsured) return;

            await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            _containerEnsured = true;
        }
        finally
        {
            _containerLock.Release();
        }
    }

    private static string GenerateFileReference(string fileName)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        return $"{Guid.NewGuid()}/{sanitizedFileName}";
    }

    private static string SanitizeFileName(string fileName)
    {
        // Azure Blob names can contain most characters, but we sanitize for consistency
        var invalidChars = new[] { '\\', ':', '*', '?', '"', '<', '>', '|' };
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(sanitized) ? "file" : sanitized;
    }
}
