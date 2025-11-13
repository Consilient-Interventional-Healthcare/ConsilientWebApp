namespace Consilient.Visits.Contracts.Results
{
    public class UploadAssignmentResult(bool succeeded, string? message)
    {
        public bool Succeeded { get; private set; } = succeeded;
        public string? Message { get; private set; } = message;
    }
}
