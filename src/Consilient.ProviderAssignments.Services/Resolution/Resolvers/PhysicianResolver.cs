using Consilient.Common;
using Consilient.Data;
using Consilient.Data.Entities.Staging;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers;

/// <summary>
/// Resolves attending physicians from provider assignments.
/// </summary>
internal class PhysicianResolver(IResolutionCache cache, ConsilientDbContext dbContext, ILogger<PhysicianResolver> logger)
    : ProviderResolver<PhysicianResolver>(cache, dbContext, logger), IPhysicianResolver
{
    protected override ProviderType TargetProviderType => ProviderType.Physician;

    protected override string GetLastNameFromRecord(ProviderAssignment record) => record.NormalizedPhysicianLastName ?? string.Empty;

    protected override void SetResolvedProviderIdOnRecord(ProviderAssignment record, int providerId)
        => record.ResolvedPhysicianId = providerId;
}
