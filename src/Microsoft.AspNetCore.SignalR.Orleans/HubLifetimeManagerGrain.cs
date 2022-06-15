using Orleans;
using Orleans.Runtime;

namespace Microsoft.AspNetCore.SignalR;

internal class HubLifetimeManagerGrain<T> : Grain<SubscriptionState>, IHubLifetimeManagerGrain<T>
{
    public Task AddToGroupAsync(string connectionId, string groupName)
    {
        return DoAction(static (s, state) =>
        {
            var (connectionId, groupName) = state;

            return s.AddToGroupAsync(connectionId, groupName);

        }, (connectionId, groupName));
    }

    public Task RemoveFromGroupAsync(string connectionId, string groupName)
    {
        return DoAction(static (s, state) =>
        {
            var (connectionId, groupName) = state;

            return s.RemoveFromGroupAsync(connectionId, groupName);

        }, (connectionId, groupName));
    }

    public Task SendAllAsync(string methodName, object?[] args)
    {
        return DoAction(static (s, state) =>
        {
            var (methodName, args) = state;

            return s.SendAllAsync(methodName, args);

        }, (methodName, args));
    }

    public Task SendAllExceptAsync(string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds)
    {
        return DoAction(static (s, state) =>
        {
            var (methodName, args, excludedConnectionIds) = state;

            return s.SendAllExceptAsync(methodName, args, excludedConnectionIds);

        }, (methodName, args, excludedConnectionIds));
    }

    public Task SendConnectionAsync(string connectionId, string methodName, object?[] args)
    {
        return DoAction(static (s, state) =>
        {
            var (connectionId, methodName, args) = state;

            return s.SendConnectionAsync(connectionId, methodName, args);

        }, (connectionId, methodName, args));
    }

    public Task SendGroupAsync(string groupName, string methodName, object?[] args)
    {
        return DoAction(static (s, state) =>
        {
            var (groupName, methodName, args) = state;

            return s.SendGroupAsync(groupName, methodName, args);

        }, (groupName, methodName, args));
    }

    public Task SendGroupExceptAsync(string groupName, string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds)
    {
        return DoAction(static (s, state) =>
        {
            var (groupName, methodName, args, excludedConnectionIds) = state;

            return s.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds);

        }, (groupName, methodName, args, excludedConnectionIds));
    }

    public Task SendUserAsync(string userId, string methodName, object?[] args)
    {
        return DoAction(static (s, state) =>
        {
            var (userId, methodName, args) = state;

            return s.SendUserAsync(userId, methodName, args);

        }, (userId, methodName, args));
    }

    public Task SubscribeAsync(IHubLifetimeManagerGrainObserver observer)
    {
        State.Subscriptions.Add(observer);
        return WriteStateAsync();
    }

    public Task UnsubscribeAsync(IHubLifetimeManagerGrainObserver observer)
    {
        State.Subscriptions.Remove(observer);
        return WriteStateAsync();
    }

    private async Task DoAction<TState>(Func<IHubLifetimeManagerGrainObserver, TState, Task> callback, TState state)
    {
        List<IHubLifetimeManagerGrainObserver>? clientsToRemove = null;

        foreach (var s in State.Subscriptions)
        {
            try
            {
                await callback(s, state);
            }
            catch (ClientNotAvailableException)
            {
                clientsToRemove ??= new();
                clientsToRemove.Add(s);
            }
        }

        if (clientsToRemove is not null)
        {
            foreach (var s in clientsToRemove)
            {
                State.Subscriptions.Remove(s);
            }

            await WriteStateAsync();
        }
    }
}

internal class SubscriptionState
{
    public HashSet<IHubLifetimeManagerGrainObserver> Subscriptions { get; } = new();
}