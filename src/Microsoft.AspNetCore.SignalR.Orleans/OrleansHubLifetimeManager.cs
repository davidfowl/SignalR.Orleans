using Microsoft.Extensions.Logging;
using Orleans;

namespace Microsoft.AspNetCore.SignalR;

internal class OrleansHubLifetimeManager<THub> : HubLifetimeManager<THub>, IHubLifetimeManagerGrainObserver, IAsyncDisposable where THub : Hub
{
    private readonly DefaultHubLifetimeManager<THub> _thisManager;
    private readonly IGrainFactory _grainFactory;
    private readonly IHubLifetimeManagerGrain<THub> _hubGrain;
    private readonly SemaphoreSlim _initialLock = new(1, 1);
    private IHubLifetimeManagerGrainObserver? _thisObserver;

    public OrleansHubLifetimeManager(IGrainFactory grainFactory, ILogger<DefaultHubLifetimeManager<THub>> logger)
    {
        _grainFactory = grainFactory;
        _thisManager = new(logger);

        _hubGrain = _grainFactory.GetGrain<IHubLifetimeManagerGrain<THub>>(typeof(THub).FullName);
    }

    public override async Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        var group = _grainFactory.GetGrain<IHubLifetimeManagerGrain<THub>>(groupName);

        await group.SubscribeAsync(_thisObserver!);

        await group.AddToGroupAsync(connectionId, groupName);
    }

    public override async Task OnConnectedAsync(HubConnectionContext connection)
    {
        await EnsureObserverAsync();

        var connectionGrain = _grainFactory.GetGrain<IHubLifetimeManagerGrain<THub>>(connection.ConnectionId);

        await connectionGrain.SubscribeAsync(_thisObserver!);

        if (connection.UserIdentifier is not null)
        {
            var userGrain = _grainFactory.GetGrain<IHubLifetimeManagerGrain<THub>>(connection.UserIdentifier);

            await userGrain.SubscribeAsync(_thisObserver!);

            // await userGrain.AddToUserAsync(connection.ConnectionId, connection.UserIdentifier);
        }

        await _thisManager.OnConnectedAsync(connection);
    }

    public override async Task OnDisconnectedAsync(HubConnectionContext connection)
    {
        var connectionGrain = _grainFactory.GetGrain<IHubLifetimeManagerGrain<THub>>(connection.ConnectionId);

        await connectionGrain.UnsubscribeAsync(_thisObserver!);

        if (connection.UserIdentifier is not null)
        {
            // var userGrain = _grainFactory.GetGrain<IHubLifetimeManagerGrain<THub>>(connection.UserIdentifier);

            // TODO: Handle removal of users
            // await userGrain.RemoveFromUserAsync(connection.ConnectionId, connection.UserIdentifier);
        }

        await _thisManager.OnDisconnectedAsync(connection);
    }

    public override async Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        var groupGrain = _grainFactory.GetGrain<IHubLifetimeManagerGrain<THub>>(groupName);

        await groupGrain.RemoveFromGroupAsync(connectionId, groupName);

        await groupGrain.UnsubscribeAsync(_thisObserver!);
    }

    public override async Task SendAllAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        await _hubGrain.SendAllAsync(methodName, args);
    }

    public override Task SendAllExceptAsync(string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
    {
        return _hubGrain.SendAllExceptAsync(methodName, args, excludedConnectionIds);
    }

    public override Task SendConnectionAsync(string connectionId, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        var connectionGrain = _grainFactory.GetGrain<IHubLifetimeManagerGrain<THub>>(connectionId);

        return connectionGrain.SendConnectionAsync(connectionId, methodName, args);
    }

    public override Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        var tasks = new Task[connectionIds.Count];
        var i = 0;

        foreach (var id in connectionIds)
        {
            tasks[i++] = SendConnectionAsync(id, methodName, args, cancellationToken);
        }
        return Task.WhenAll(tasks);
    }

    public override Task SendGroupAsync(string groupName, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        var group = _grainFactory.GetGrain<IHubLifetimeManagerGrain<THub>>(groupName);

        return group.SendGroupAsync(groupName, methodName, args);
    }

    public override Task SendGroupExceptAsync(string groupName, string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
    {
        var group = _grainFactory.GetGrain<IHubLifetimeManagerGrain<THub>>(groupName);

        return group.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds);
    }

    public override Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        var tasks = new Task[groupNames.Count];
        var i = 0;

        foreach (var group in groupNames)
        {
            tasks[i++] = SendGroupAsync(group, methodName, args, cancellationToken);
        }

        return Task.WhenAll(tasks);
    }

    public override Task SendUserAsync(string userId, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        var userGrain = _grainFactory.GetGrain<IHubLifetimeManagerGrain<THub>>(userId);

        return userGrain.SendUserAsync(userId, methodName, args);
    }

    public override Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object?[] args, CancellationToken cancellationToken = default)
    {
        var tasks = new Task[userIds.Count];
        var i = 0;

        foreach (var userId in userIds)
        {
            tasks[i++] = SendUserAsync(userId, methodName, args, cancellationToken);
        }

        return Task.WhenAll(tasks);
    }

    Task IHubLifetimeManagerGrainObserver.SendAllAsync(string methodName, object?[] args)
    {
        return _thisManager.SendAllAsync(methodName, args);
    }

    Task IHubLifetimeManagerGrainObserver.SendAllExceptAsync(string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds)
    {
        return _thisManager.SendAllExceptAsync(methodName, args, excludedConnectionIds);
    }

    Task IHubLifetimeManagerGrainObserver.SendConnectionAsync(string connectionId, string methodName, object?[] args)
    {
        return _thisManager.SendConnectionAsync(connectionId, methodName, args);
    }

    Task IHubLifetimeManagerGrainObserver.SendGroupAsync(string groupName, string methodName, object?[] args)
    {
        return _thisManager.SendGroupAsync(groupName, methodName, args);
    }

    Task IHubLifetimeManagerGrainObserver.SendGroupExceptAsync(string groupName, string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds)
    {
        return _thisManager.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds);
    }

    Task IHubLifetimeManagerGrainObserver.AddToGroupAsync(string connectionId, string groupName)
    {
        // This will noop if the connection isn't on this node
        return _thisManager.AddToGroupAsync(connectionId, groupName);
    }

    Task<bool> IHubLifetimeManagerGrainObserver.RemoveFromGroupAsync(string connectionId, string groupName)
    {
        // This will noop if the connection isn't on this node
        _thisManager.RemoveFromGroupAsync(connectionId, groupName);

        // REVIEW: We need to track group -> connection count on this node
        return Task.FromResult(true);
    }

    Task IHubLifetimeManagerGrainObserver.SendUserAsync(string userId, string methodName, object?[] args)
    {
        return _thisManager.SendUserAsync(userId, methodName, args);
    }

    private async Task EnsureObserverAsync()
    {
        if (_thisObserver is null)
        {
            await _initialLock.WaitAsync();

            try
            {
                if (_thisObserver is not null)
                {
                    // Somebody else set the observer
                    return;
                }

                _thisObserver = await _grainFactory.CreateObjectReference<IHubLifetimeManagerGrainObserver>(this);

                await _hubGrain.SubscribeAsync(_thisObserver);
            }
            finally
            {
                _initialLock.Release();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_thisObserver is not null)
        {
            await _hubGrain.UnsubscribeAsync(_thisObserver);
        }
    }
}
