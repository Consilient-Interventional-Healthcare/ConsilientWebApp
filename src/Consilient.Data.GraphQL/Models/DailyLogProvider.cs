using Consilient.Common;

namespace Consilient.Data.GraphQL.Models;

public class DailyLogProvider
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public ProviderType Type { get; set; }
}
