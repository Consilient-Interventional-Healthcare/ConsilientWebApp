using Microsoft.AspNetCore.SignalR;

namespace Consilient.Api.Hubs;

public class ProgressHub : Hub
{
    public async Task JoinProgressGroup(string jobId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
    }

    public async Task LeaveProgressGroup(string jobId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, jobId);
    }
}