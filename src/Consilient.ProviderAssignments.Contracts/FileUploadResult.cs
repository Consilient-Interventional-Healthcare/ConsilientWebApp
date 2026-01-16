namespace Consilient.ProviderAssignments.Contracts
{
    public class FileUploadResult
    {
        public string FileName { get; set; } = null!;
        public DateOnly ServiceDate { get; set; }
        public int FacilityId { get; set; }
        public string Message { get; set; } = null!;
        public Guid BatchId { get; set; }
    }
}
