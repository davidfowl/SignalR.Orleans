using Orleans;
using Orleans.Runtime;

namespace Microsoft.AspNetCore.SignalR;

internal class HubLifetimeManagerGrain<T> : Grain<HashSet<IHubLifetimeManagerGrainObserver>>, IHubLifetimeManagerGrain<T>
{
    public async Task AddToGroupAsync(string connectionId, string groupName)
    {
        foreach (var s in State)
        {
            await s.AddToGroupAsync(connectionId, groupName);
        }
    }

    public async Task RemoveFromGroupAsync(string connectionId, string groupName)
    {
        foreach (var s in State)
        {
            if (!await s.RemoveFromGroupAsync(connectionId, groupName))
            {
                // If this is the last connection in the group then unsubscribe
                State.Remove(s);
            }
        }
    }

    public async Task SendAllAsync(string methodName, object?[] args)
    {
        foreach (var s in State)
        {
            await s.SendAllAsync(methodName, args);
        }
    }

    public async Task SendAllExceptAsync(string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds)
    {
        foreach (var s in State)
        {
            await s.SendAllExceptAsync(methodName, args, excludedConnectionIds);
        }
    }

    public async Task SendConnectionAsync(string connectionId, string methodName, object?[] args)
    {
        foreach (var s in State)
        {
            await s.SendConnectionAsync(connectionId, methodName, args);
        }
    }

    public async Task SendGroupAsync(string groupName, string methodName, object?[] args)
    {
        foreach (var s in State)
        {
            await s.SendGroupAsync(groupName, methodName, args);
        }
    }

    public async Task SendGroupExceptAsync(string groupName, string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds)
    {
        foreach (var s in State)
        {
            await s.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds);
        }
    }

    public async Task SendUserAsync(string userId, string methodName, object?[] args)
    {
        foreach (var s in State)
        {
            await s.SendUserAsync(userId, methodName, args);
        }
    }

    public Task SubscribeAsync(IHubLifetimeManagerGrainObserver observer)
    {
        State.Add(observer);
        return WriteStateAsync();
    }

    public Task UnsubscribeAsync(IHubLifetimeManagerGrainObserver observer)
    {
        State.Remove(observer);
        return WriteStateAsync();
    }
}