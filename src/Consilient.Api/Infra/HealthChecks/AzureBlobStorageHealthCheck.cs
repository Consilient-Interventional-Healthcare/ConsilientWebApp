using Azure;
using Consilient.Infrastructure.Storage;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace Consilient.Api.Infra.HealthChecks;

/// <summary>
/// Health check for Azure Blob Storage that verifies:
/// 1. Connectivity - container is reachable
/// 2. Write access - can upload a test blob
/// 3. Read access - can retrieve the test blob
/// 4. Cleanup - can delete the test blob
/// </summary>
internal class AzureBlobStorageHealthCheck(AzureBlobFileStorage fileStorage) : IHealthCheck
{
    private readonly AzureBlobFileStorage _fileStorage = fileStorage;
    private readonly Serilog.ILogger _logger = Log.Logger;

    // Cache the last successful check to avoid excessive operations
    private static DateTime _lastSuccessfulCheck = DateTime.MinValue;
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();

        // Return cached result if recent check was successful
        if (DateTime.UtcNow - _lastSuccessfulCheck < _cacheDuration)
        {
            data["status"] = "cached";
            data["lastCheck"] = _lastSuccessfulCheck.ToString("o");
            return HealthCheckResult.Healthy(
                $"Azure Blob Storage healthy (verified at {_lastSuccessfulCheck:HH:mm:ss} UTC)",
                data: data);
        }

        var testFileName = $".healthcheck-{Guid.NewGuid()}.txt";
        var testContent = $"Health check test at {DateTime.UtcNow:o}";
        string? fileReference = null;

        try
        {
            // Step 1: Write test blob
            data["write"] = "attempting";
            using (var contentStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testContent)))
            {
                fileReference = await _fileStorage.SaveAsync(testFileName, contentStream, cancellationToken);
            }
            data["write"] = "success";
            data["fileReference"] = fileReference;

            // Step 2: Read test blob
            data["read"] = "attempting";
            using (var retrievedStream = await _fileStorage.GetAsync(fileReference, cancellationToken))
            using (var reader = new StreamReader(retrievedStream))
            {
                var retrievedContent = await reader.ReadToEndAsync(cancellationToken);
                if (retrievedContent != testContent)
                {
                    data["read"] = "failed";
                    data["error"] = "Retrieved content does not match written content";
                    return HealthCheckResult.Unhealthy(
                        "Azure Blob Storage read verification failed: content mismatch",
                        data: data);
                }
            }
            data["read"] = "success";

            // Step 3: Verify existence
            data["exists"] = "attempting";
            var exists = await _fileStorage.ExistsAsync(fileReference, cancellationToken);
            if (!exists)
            {
                data["exists"] = "failed";
                data["error"] = "Blob should exist but ExistsAsync returned false";
                return HealthCheckResult.Degraded(
                    "Azure Blob Storage existence check failed",
                    data: data);
            }
            data["exists"] = "success";

            // Step 4: Delete test blob
            data["delete"] = "attempting";
            await _fileStorage.DeleteAsync(fileReference, cancellationToken);
            data["delete"] = "success";

            // Verify deletion
            var existsAfterDelete = await _fileStorage.ExistsAsync(fileReference, cancellationToken);
            if (existsAfterDelete)
            {
                data["delete"] = "warning";
                data["warning"] = "Blob still exists after deletion";
                return HealthCheckResult.Degraded(
                    "Azure Blob Storage delete verification failed: blob still exists",
                    data: data);
            }

            // All checks passed
            _lastSuccessfulCheck = DateTime.UtcNow;
            data["status"] = "verified";
            data["lastCheck"] = _lastSuccessfulCheck.ToString("o");

            return HealthCheckResult.Healthy(
                "Azure Blob Storage healthy (write, read, delete verified)",
                data: data);
        }
        catch (RequestFailedException ex)
        {
            data["error"] = ex.Message;
            data["errorCode"] = ex.ErrorCode ?? string.Empty;
            data["status"] = ex.Status;

            _logger.Warning(ex, "Azure Blob Storage health check failed with RequestFailedException");

            return HealthCheckResult.Unhealthy(
                $"Azure Blob Storage unhealthy: {ex.Message}",
                ex,
                data: data);
        }
        catch (Exception ex)
        {
            data["error"] = ex.Message;
            data["errorType"] = ex.GetType().Name;

            _logger.Warning(ex, "Azure Blob Storage health check failed");

            return HealthCheckResult.Unhealthy(
                $"Azure Blob Storage unhealthy: {ex.Message}",
                ex,
                data: data);
        }
        finally
        {
            // Cleanup: ensure test blob is deleted even if checks fail
            if (!string.IsNullOrEmpty(fileReference))
            {
                try
                {
                    await _fileStorage.DeleteAsync(fileReference, cancellationToken);
                }
                catch (Exception cleanupEx)
                {
                    _logger.Debug(cleanupEx, "Failed to cleanup test blob {FileReference}", fileReference);
                }
            }
        }
    }
}
