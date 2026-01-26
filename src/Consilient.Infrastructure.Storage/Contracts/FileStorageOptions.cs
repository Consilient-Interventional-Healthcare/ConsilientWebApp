namespace Consilient.Infrastructure.Storage.Contracts
{
    /// <summary>
    /// Configuration options for file storage.
    /// </summary>
    public class FileStorageOptions
    {
        public const string SectionName = "FileStorage";

        /// <summary>
        /// The storage provider to use: "Local" or "AzureBlob".
        /// </summary>
        public string Provider { get; init; } = "Local";

        /// <summary>
        /// The local file system path for storing files (used when Provider is "Local").
        /// If not specified, defaults to a temp directory.
        /// </summary>
        public string? LocalPath { get; init; }

        /// <summary>
        /// The Azure Blob Storage connection string (used when Provider is "AzureBlob").
        /// </summary>
        public string? AzureBlobConnectionString { get; init; }

        /// <summary>
        /// The Azure Blob Storage container name. Defaults to "uploads".
        /// </summary>
        public string ContainerName { get; init; } = "uploads";
    }
}
