using Consilient.Common.Contracts;

namespace Consilient.Common.Services;

/// <summary>
/// No-op implementation of IUserContextSetter for use in API context.
/// The API uses ICurrentUserService from HttpContext, so SetUser is not needed.
/// This exists only to satisfy Hangfire's dependency resolution during job enqueueing.
/// </summary>
public class NoOpUserContextSetter : IUserContextSetter
{
    public void SetUser(int userId, string? userName = null, string? userEmail = null)
    {
        // No-op: In API context, user context comes from HttpContext
    }
}
