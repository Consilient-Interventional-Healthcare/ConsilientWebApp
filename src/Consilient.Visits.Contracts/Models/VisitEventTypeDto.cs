namespace Consilient.Visits.Contracts.Models
{
    public class VisitEventTypeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? IconName { get; set; }
        public string? Color { get; set; }
    }
}