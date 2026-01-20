using Consilient.Common;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers
{
    internal record ProviderRow
    {
        public int ProviderId { get; init; }
        public string ProviderLastName { get; init; } = string.Empty;
        public string ProviderFirstName { get; init; } = string.Empty;
        public ProviderType ProviderType { get; init; }
    }
}
