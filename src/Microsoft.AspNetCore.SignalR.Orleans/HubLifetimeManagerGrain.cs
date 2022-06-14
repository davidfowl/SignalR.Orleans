using Orleans;

namespace Microsoft.AspNetCore.SignalR;

internal class HubLifetimeManagerGrain<T> : Grain, IHubLifetimeManagerGrain<T>
{
    private readonly HashSet<IHubLifetimeManagerGrainObserver> _subs = new();

    public async Task AddToGroupAsync(string connectionId, string groupName)
    {
        foreach (var s in _subs)
        {
            await s.AddToGroupAsync(connectionId, groupName);
        }
    }

    public async Task RemoveFromGroupAsync(string connectionId, string groupName)
    {
        foreach (var s in _subs)
        {
            if (!await s.RemoveFromGroupAsync(connectionId, groupName))
            {
                // If this is the last connection in the group then unsubscribe
                _subs.Remove(s);
            }
        }
    }

    public async Task SendAllAsync(string methodName, object?[] args)
    {
        foreach (var s in _subs)
        {
            await s.SendAllAsync(methodName, args);
        }
    }

    public async Task SendAllExceptAsync(string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds)
    {
        foreach (var s in _subs)
        {
            await s.SendAllExceptAsync(methodName, args, excludedConnectionIds);
        }
    }

    public async Task SendConnectionAsync(string connectionId, string methodName, object?[] args)
    {
        foreach (var s in _subs)
        {
            await s.SendConnectionAsync(connectionId, methodName, args);
        }
    }

    public async Task SendGroupAsync(string groupName, string methodName, object?[] args)
    {
        foreach (var s in _subs)
        {
            await s.SendGroupAsync(groupName, methodName, args);
        }
    }

    public async Task SendGroupExceptAsync(string groupName, string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds)
    {
        foreach (var s in _subs)
        {
            await s.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds);
        }
    }

    public async Task SendUserAsync(string userId, string methodName, object?[] args)
    {
        foreach (var s in _subs)
        {
            await s.SendUserAsync(userId, methodName, args);
        }
    }

    public Task SubscribeAsync(IHubLifetimeManagerGrainObserver observer)
    {
        _subs.Add(observer);
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(IHubLifetimeManagerGrainObserver observer)
    {
        _subs.Remove(observer);
        return Task.CompletedTask;
    }
}
