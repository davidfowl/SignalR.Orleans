using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs;

public class Chat : Hub
{
    public async Task Send(string message)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "g1");

        await Clients.All.SendAsync("Send", message);

        await Clients.Group("g1").SendAsync("Send", "To the group!");
    }
}
