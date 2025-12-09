namespace Consilient.Common.Services
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string? UserEmail { get; }
        string? UserName { get; }
    }
}