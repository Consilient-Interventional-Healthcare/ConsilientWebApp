namespace Consilient.WebApp.Infra
{
    public interface ICurrentUserService
    {
        /// <summary>
        /// Returns the current user's id claim value (string) or null if no user / claim.
        /// </summary>
        string? UserId { get; }

        string? UserEmail { get; }
    }
}