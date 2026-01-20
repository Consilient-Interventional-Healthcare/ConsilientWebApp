using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Consilient.ProviderAssignments.Services.Resolution.Resolvers;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution
{
    internal class ProviderAssignmentsResolver(
        IResolverProvider resolverProvider,
        ConsilientDbContext dbContext,
        ILogger<ProviderAssignmentsResolver> logger) : IProviderAssignmentsResolver
    {
        private readonly IResolverProvider _resolverProvider = resolverProvider;
        private readonly ConsilientDbContext _dbContext = dbContext;
        private readonly ILogger<ProviderAssignmentsResolver> _logger = logger;


        // Resolution stages for progress reporting
        private static readonly string[] ResolutionStages =
        [
            "Physician",
            "NursePractitioner",
            "Patient",
            "Hospitalization",
            "Visit",
            "BulkUpdate"
        ];

        public async Task ResolveAsync(
            Guid batchId,
            int facilityId,
            DateOnly date,
            CancellationToken cancellationToken = default,
            IProgress<ResolutionProgressEventArgs>? progress = null)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Step 0: Validate batch exists and belongs to the specified facility
                    await ValidateBatchAsync(batchId, facilityId, cancellationToken);

                    // Step 1: Load staging records
                    var recordsList = await GetStagingRecordsForResolution(batchId, cancellationToken);

                    _logger.LogInformation("Batch {BatchId}: Loaded {RecordCount} staging records", batchId, recordsList.Count);

                    if (recordsList.Count != 0)
                    {
                        // Step 2: Create cache once for the entire resolution cycle
                        var cache = new ResolutionCache();

                        // Step 3: Run all resolvers in dependency order
                        await ResolveAll(cache, facilityId, date, recordsList, batchId, progress);

                        // Step 4: Bulk update staging table
                        ReportProgress(progress, "BulkUpdate", recordsList.Count, recordsList.Count, batchId, 6, ResolutionStages.Length);
                        await BulkUpdateAllChanges(recordsList, cancellationToken);
                    }

                    _logger.LogInformation("Batch {BatchId}: Resolution completed", batchId);

                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Batch {BatchId}: Resolution failed, rolling back", batchId);
                    await transaction.RollbackAsync(cancellationToken);
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
            List<ProviderAssignment> records,
            Guid batchId,
            IProgress<ResolutionProgressEventArgs>? progress)
        {
            var totalRecords = records.Count;
            var totalSteps = ResolutionStages.Length;

            ReportProgress(progress, "Physician", 0, totalRecords, batchId, 1, totalSteps);
            await RunResolvers<IPhysicianResolver>(cache, facilityId, date, records);

            ReportProgress(progress, "NursePractitioner", 0, totalRecords, batchId, 2, totalSteps);
            await RunResolvers<INursePractitionerResolver>(cache, facilityId, date, records);

            ReportProgress(progress, "Patient", 0, totalRecords, batchId, 3, totalSteps);
            await RunResolvers<IPatientResolver>(cache, facilityId, date, records);

            ReportProgress(progress, "Hospitalization", 0, totalRecords, batchId, 4, totalSteps);
            await RunResolvers<IHospitalizationResolver>(cache, facilityId, date, records);

            ReportProgress(progress, "Visit", 0, totalRecords, batchId, 5, totalSteps);
            await RunResolvers<IVisitResolver>(cache, facilityId, date, records);
        }

        private async Task RunResolvers<TResolver>(IResolutionCache cache, int facilityId, DateOnly date, List<ProviderAssignment> records)
            where TResolver : IResolver
        {
            foreach (var resolver in _resolverProvider.GetResolvers<TResolver>(cache, _dbContext))
            {
                await resolver.ResolveAsync(facilityId, date, records);
            }
        }

        private async Task BulkUpdateAllChanges(List<ProviderAssignment> records, CancellationToken cancellationToken)
        {
            await _dbContext.BulkUpdateAsync(records, cancellationToken: cancellationToken);
        }

        private async Task<List<ProviderAssignment>> GetStagingRecordsForResolution(Guid batchId, CancellationToken cancellationToken = default)
        {
            // Only load records without validation errors - validation happens during import
            return await _dbContext.StagingProviderAssignments
                .Where(x => x.BatchId == batchId && x.ValidationErrorsJson == null)
                .ToListAsync(cancellationToken);
        }

        private async Task ValidateBatchAsync(Guid batchId, int facilityId, CancellationToken cancellationToken)
        {
            var batchExists = await _dbContext.StagingProviderAssignments
                .AnyAsync(x => x.BatchId == batchId, cancellationToken);

            if (!batchExists)
            {
                throw new InvalidOperationException($"Batch {batchId} not found in staging table");
            }

            var facilityMismatch = await _dbContext.StagingProviderAssignments
                .AnyAsync(x => x.BatchId == batchId && x.FacilityId != facilityId, cancellationToken);

            if (facilityMismatch)
            {
                throw new InvalidOperationException($"Batch {batchId} contains records for a different facility than {facilityId}");
            }
        }

        private static void ReportProgress(
            IProgress<ResolutionProgressEventArgs>? progress,
            string stage,
            int processedRecords,
            int totalRecords,
            Guid batchId,
            int currentStep,
            int totalSteps)
        {
            progress?.Report(new ResolutionProgressEventArgs
            {
                Stage = stage,
                ProcessedRecords = processedRecords,
                TotalRecords = totalRecords,
                BatchId = batchId,
                CurrentStep = currentStep,
                TotalSteps = totalSteps
            });
        }
    }
}
