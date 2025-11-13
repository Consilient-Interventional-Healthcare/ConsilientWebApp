namespace Consilient.Visits.Contracts.Requests
{
    public class CreateVisitRequest
    {
        public DateOnly DateServiced { get; set; }
        public int HospitalizationId { get; set; }
        public int ServiceTypeId { get; set; }
        public bool IsScribeServiceOnly { get; set; }
    }
}