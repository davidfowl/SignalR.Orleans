using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

var connection = new HubConnectionBuilder()
                   .WithUrl("https://localhost:7181/chat")
                   .ConfigureLogging(logging =>
                   {
                       logging.SetMinimumLevel(LogLevel.Information);
                       logging.AddConsole();
                   })
                   .Build();

await Task.Delay(1000);

connection.On("Send", (string name) =>
{
    Console.WriteLine($"Server: {name}");
});

await connection.StartAsync();


while (true)
{
    Console.Write("> ");
    var message = Console.ReadLine();
    await connection.InvokeAsync("Send", message);
}