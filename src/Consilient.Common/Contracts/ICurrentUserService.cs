namespace Consilient.Common.Contracts
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string? UserEmail { get; }
        string? UserName { get; }
    }
}