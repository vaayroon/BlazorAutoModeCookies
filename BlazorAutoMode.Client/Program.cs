using BlazorAutoMode.Client;
using BlazorAutoMode.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

// Register client-side error handler
builder.Services.AddScoped<IClientErrorHandler, ClientErrorHandler>();

// Configure error logging
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// Setup global unhandled exception handler
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    if (args.ExceptionObject is Exception ex)
    {
        Console.Error.WriteLine($"Unhandled WebAssembly exception: {ex}");
    }
    else
    {
        Console.Error.WriteLine($"Unhandled WebAssembly exception: {args.ExceptionObject}");
    }
};

var host = builder.Build();

// Initialize the client error handler
var serviceProvider = host.Services;
await using var scope = serviceProvider.CreateAsyncScope();
try
{
    var errorHandler = scope.ServiceProvider.GetRequiredService<IClientErrorHandler>();
    await errorHandler.InitializeAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to initialize client error handler: {ex}");
}

await host.RunAsync();
