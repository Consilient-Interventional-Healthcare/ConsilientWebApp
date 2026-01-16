namespace Consilient.Common.Services
{
    public interface IUserContextSetter
    {
        void SetUser(int userId, string? userName = null, string? userEmail = null);
    }
}
