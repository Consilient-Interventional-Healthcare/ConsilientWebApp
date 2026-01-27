namespace Consilient.Infrastructure.Storage.Contracts;

/// <summary>
/// Abstraction for file storage operations, supporting local filesystem and Azure Blob Storage.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Saves a file and returns a reference string that can be used to retrieve it later.
    /// </summary>
    /// <param name="fileName">The original file name (used for reference generation).</param>
    /// <param name="content">The file content stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A file reference string that can be used to retrieve or delete the file.</returns>
    Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a file by its reference. Caller is responsible for disposing the stream.
    /// </summary>
    /// <param name="fileReference">The reference returned from SaveAsync.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A readable stream containing the file contents.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    Task<Stream> GetAsync(string fileReference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file by its reference. Does not throw if file doesn't exist.
    /// </summary>
    /// <param name="fileReference">The reference returned from SaveAsync.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(string fileReference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="fileReference">The reference returned from SaveAsync.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    Task<bool> ExistsAsync(string fileReference, CancellationToken cancellationToken = default);
}
