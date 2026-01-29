namespace Consilient.Data.Entities.Clinical;

public class Patient : BaseEntity<int>
{
    public DateOnly? BirthDate { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public string LastName { get; set; } = string.Empty;

    public virtual ICollection<PatientFacility> PatientFacilities { get; set; } = [];
}