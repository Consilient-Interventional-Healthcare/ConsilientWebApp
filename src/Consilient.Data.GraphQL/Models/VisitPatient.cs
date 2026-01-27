namespace Consilient.Data.GraphQL.Models;

public class VisitPatient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string Mrn { get; set; } = string.Empty;
}
