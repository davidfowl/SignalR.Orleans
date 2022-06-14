using Orleans;
using Orleans.Hosting;
using Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(builder => builder.UseLocalhostClustering().AddMemoryGrainStorageAsDefault());

builder.Services.AddSignalR().AddOrleans();

var app = builder.Build();

app.MapHub<Chat>("/chat");

app.Run();
