using Consilient.Data;
using Consilient.ProviderAssignments.Contracts.Processing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Consilient.ProviderAssignments.Services.Processing
{
    internal class ProviderAssignmentsProcessor(
        ConsilientDbContext dbContext,
        ILogger<ProviderAssignmentsProcessor> logger) : IProviderAssignmentsProcessor
    {
        private const string ProcessStoredProcedure = "[staging].[usp_ProcessProviderAssignments]";

        private readonly ConsilientDbContext _dbContext = dbContext;
        private readonly ILogger<ProviderAssignmentsProcessor> _logger = logger;

        public async Task<ProcessResult> ProcessAsync(Guid batchId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Batch {BatchId}: Starting processing with SP {StoredProcedure}",
                batchId, ProcessStoredProcedure);

            var connection = _dbContext.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            using var command = connection.CreateCommand();
            command.CommandText = ProcessStoredProcedure;
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = _dbContext.Database.CurrentTransaction?.GetDbTransaction();

            // Input parameter
            command.Parameters.Add(new SqlParameter("@BatchId", SqlDbType.UniqueIdentifier) { Value = batchId });

            // Output parameters
            var processedCountParam = new SqlParameter("@ProcessedCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var errorCountParam = new SqlParameter("@ErrorCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

            command.Parameters.Add(processedCountParam);
            command.Parameters.Add(errorCountParam);
            command.Parameters.Add(errorMessageParam);

            await command.ExecuteNonQueryAsync(cancellationToken);

            var processedCount = processedCountParam.Value == DBNull.Value ? 0 : (int)processedCountParam.Value;
            var errorCount = errorCountParam.Value == DBNull.Value ? 0 : (int)errorCountParam.Value;
            var errorMessage = errorMessageParam.Value == DBNull.Value ? null : (string?)errorMessageParam.Value;

            _logger.LogInformation(
                "Batch {BatchId}: SP completed - {ProcessedCount} processed, {ErrorCount} errors",
                batchId, processedCount, errorCount);

            return new ProcessResult(processedCount, errorCount, errorMessage);
        }
    }
}
