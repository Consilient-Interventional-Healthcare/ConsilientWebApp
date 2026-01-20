namespace Consilient.Background.Workers.Models
{
    public class ProviderAssignmentsImportInput
    {
        public required string FileReference { get; init; }
        public required int FacilityId { get; init; }
        public required DateOnly ServiceDate { get; init; }
        public required int EnqueuedByUserId { get; init; }
    }
}
