using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorAutoMode.Client.Services;

public interface IClientErrorHandler
{
    Task InitializeAsync();
    Task HandleExceptionAsync(Exception exception, string source);
}

public class ClientErrorHandler : IClientErrorHandler, IAsyncDisposable
{
    private readonly ILogger<ClientErrorHandler> _logger;
    private readonly NavigationManager _navigationManager;
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<ClientErrorHandler>? _dotNetRef;

    public ClientErrorHandler(
        ILogger<ClientErrorHandler> logger,
        NavigationManager navigationManager,
        IJSRuntime jsRuntime)
    {
        _logger = logger;
        _navigationManager = navigationManager;
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            
            // Use the import approach instead of direct reference to support lazy loading
            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorAutoMode.Client/js/clientErrorHandling.js");
                
            await _jsModule.InvokeVoidAsync("clientErrorHandling.initialize", _dotNetRef);
            
            _logger.LogInformation("Client-side error handler initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize client-side error handler");
        }
    }
    
    public Task HandleExceptionAsync(Exception exception, string source)
    {
        _logger.LogError(exception, "Client-side error occurred in {Source}", source);
        
        // For fatal errors, redirect to error page
        if (IsFatalException(exception))
        {
            try
            {
                _navigationManager.NavigateTo("/Error", forceLoad: true);
            }
            catch (Exception navEx)
            {
                _logger.LogError(navEx, "Failed to navigate to error page");
            }
        }
        
        return Task.CompletedTask;
    }
    
    [JSInvokable]
    public Task HandleJavaScriptErrorAsync(string message, string source, int line, int column, string? stack)
    {
        _logger.LogError(
            "Client JavaScript error: {Message} at {Source}:{Line}:{Column}\nStack: {Stack}",
            message, source, line, column, stack ?? "No stack trace available"
        );
        
        return Task.CompletedTask;
    }
    
    private bool IsFatalException(Exception exception)
    {
        // Consider certain exceptions as fatal and requiring a page reload
        return exception is OutOfMemoryException || 
               exception.Message.Contains("Circuit", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            await _jsModule.DisposeAsync();
        }
        
        _dotNetRef?.Dispose();
    }
}
