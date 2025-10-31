namespace Consilient.Patients.Contracts.Results
{
    public class UploadSpreadsheetResult(bool succeeded, string? message)
    {
        public bool Succeeded { get; private set; } = succeeded;
        public string? Message { get; private set; } = message;
    }
}
