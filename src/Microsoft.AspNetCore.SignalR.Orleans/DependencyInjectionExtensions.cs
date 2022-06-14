using Microsoft.AspNetCore.SignalR;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static ISignalRServerBuilder AddOrleans(this ISignalRServerBuilder signalRServerBuilder)
    {
        signalRServerBuilder.Services.AddSingleton(typeof(HubLifetimeManager<>), typeof(OrleansHubLifetimeManager<>));
        return signalRServerBuilder;
    }
}
