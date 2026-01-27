namespace Consilient.Data.Entities.Clinical;

public class Facility : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;

    public string Abbreviation { get; set; } = string.Empty;

    public virtual ICollection<PatientFacility> PatientFacilities { get; set; } = [];
}