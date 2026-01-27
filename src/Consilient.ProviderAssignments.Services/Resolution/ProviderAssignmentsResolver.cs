using Consilient.Data;
using Consilient.Data.Entities.Staging;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Consilient.ProviderAssignments.Contracts.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution;

internal class ProviderAssignmentsResolver(
    IResolverProvider resolverProvider,
    ConsilientDbContext dbContext,
    ILogger<ProviderAssignmentsResolver> logger) : IProviderAssignmentsResolver
{
    private readonly IResolverProvider _resolverProvider = resolverProvider;
    private readonly ConsilientDbContext _dbContext = dbContext;
    private readonly ILogger<ProviderAssignmentsResolver> _logger = logger;


    public async Task ResolveAsync(
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Step 0: Load and validate batch
                var batch = await LoadAndValidateBatchAsync(batchId, cancellationToken);
                if (batch == null)
                {
                    return;
                }

                // Step 1: Load staging records
                var recordsList = await GetStagingRecordsForResolution(batchId, cancellationToken);

                _logger.LogInformation("Batch {BatchId}: Loaded {RecordCount} staging records", batchId, recordsList.Count);

                if (recordsList.Count != 0)
                {
                    // Step 2: Create validation contexts (deserialize any existing errors)
                    var contexts = recordsList
                        .Select(r => new RowValidationContext(r, r.ValidationErrorsJson))
                        .ToList();

                    // Step 3: Create cache once for the entire resolution cycle
                    var cache = new ResolutionCache();

                    // Step 4: Run all resolvers in dependency order
                    await ResolveAll(cache, batch.FacilityId, batch.Date, contexts);

                    // Step 5: Persist any validation errors back to the rows
                    foreach (var ctx in contexts)
                    {
                        ctx.PersistToRow();
                    }
                }

                // Step 6: Update batch status and save all changes
                //ReportProgress(progress, "SaveChanges", recordsList.Count, recordsList.Count, batchId);
                batch.Status = ProviderAssignmentBatchStatus.Resolved;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Batch {BatchId}: Resolution completed", batchId);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch {BatchId}: Resolution failed, rolling back", batchId);
                throw;
            }
        });
    }

    /// <summary>
    /// Run all resolvers in dependency order.
    /// Order is critical: Providers → Patient → Hospitalization → Visit
    /// </summary>
    private async Task ResolveAll(
        IResolutionCache cache,
        int facilityId,
        DateOnly date,
        List<RowValidationContext> contexts)
    {
        //ReportProgress(progress, "Physician", 0, totalRecords, batchId);
        await RunResolvers<IPhysicianResolver>(cache, facilityId, date, contexts);

        //ReportProgress(progress, "NursePractitioner", 0, totalRecords, batchId);
        await RunResolvers<INursePractitionerResolver>(cache, facilityId, date, contexts);

        //ReportProgress(progress, "Patient", 0, totalRecords, batchId);
        await RunResolvers<IPatientResolver>(cache, facilityId, date, contexts);

        //ReportProgress(progress, "Hospitalization", 0, totalRecords, batchId);
        await RunResolvers<IHospitalizationResolver>(cache, facilityId, date, contexts);

        //await RunResolvers<IHospitalizationStatusResolver>(cache, facilityId, date, contexts);

        //ReportProgress(progress, "Visit", 0, totalRecords, batchId);
        await RunResolvers<IVisitResolver>(cache, facilityId, date, contexts);

    }

    private async Task RunResolvers<TResolver>(IResolutionCache cache, int facilityId, DateOnly date, List<RowValidationContext> contexts)
        where TResolver : IResolver
    {
        foreach (var resolver in _resolverProvider.GetResolvers<TResolver>(cache, _dbContext))
        {
            await resolver.ResolveAsync(facilityId, date, contexts);
        }
    }

    private async Task<List<ProviderAssignment>> GetStagingRecordsForResolution(Guid batchId, CancellationToken cancellationToken = default)
    {
        // Only load records without validation errors - validation happens during import
        return await _dbContext.StagingProviderAssignments
            .Where(x => x.BatchId == batchId && x.ValidationErrorsJson == null)
            .ToListAsync(cancellationToken);
    }

    private async Task<ProviderAssignmentBatch?> LoadAndValidateBatchAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.StagingProviderAssignmentBatches.FindAsync([batchId], cancellationToken);

        if (batch == null)
        {
            _logger.LogWarning("Batch {BatchId}: Batch not found, skipping resolution", batchId);
            return null;
        }

        if (batch.Status != ProviderAssignmentBatchStatus.Imported)
        {
            _logger.LogWarning(
                "Batch {BatchId}: Skipping resolution because batch status is '{ActualStatus}' (expected '{ExpectedStatus}'). " +
                "This batch may have already been resolved or is still being imported.",
                batchId, batch.Status, ProviderAssignmentBatchStatus.Imported);
            return null;
        }

        return batch;
    }

    //private static void ReportProgress(
    //    IProgress<ResolutionProgressEventArgs>? progress,
    //    string stage,
    //    int processedRecords,
    //    int totalRecords,
    //    Guid batchId)
    //{
    //    progress?.Report(new ResolutionProgressEventArgs
    //    {
    //        Stage = stage,
    //        ProcessedRecords = processedRecords,
    //        TotalRecords = totalRecords,
    //        BatchId = batchId
    //    });
    //}
}
