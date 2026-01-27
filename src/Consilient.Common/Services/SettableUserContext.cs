using Consilient.Common.Contracts;

namespace Consilient.Common.Services;

public class SettableUserContext : ICurrentUserService, IUserContextSetter
{
    public int UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public string? UserName { get; private set; }

    public void SetUser(int userId, string? userName = null, string? userEmail = null)
    {
        UserId = userId;
        UserName = userName;
        UserEmail = userEmail;
    }
}
