using Consilient.Data;
using Consilient.Data.Entities.Clinical;
using Consilient.Data.Entities.Staging;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers;

/// <summary>
/// Resolves nurse practitioners from provider assignments.
/// </summary>
internal class NursePractitionerResolver(IResolutionCache cache, ConsilientDbContext dbContext, ILogger<NursePractitionerResolver> logger)
    : ProviderResolver<NursePractitionerResolver>(cache, dbContext, logger), INursePractitionerResolver
{
    protected override ProviderType TargetProviderType => ProviderType.NursePractitioner;

    protected override string GetLastNameFromRecord(ProviderAssignment record) => record.NormalizedNursePractitionerLastName ?? string.Empty;

    protected override void SetResolvedProviderIdOnRecord(ProviderAssignment record, int providerId)
        => record.ResolvedNursePractitionerId = providerId;
}
