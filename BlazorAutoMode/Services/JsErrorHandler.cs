using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace BlazorAutoMode.Services;

public interface IJsErrorHandler
{
    Task InitializeAsync();
    [JSInvokable]
    Task HandleJavaScriptError(JsErrorInfo errorInfo);
}

public class JsErrorHandler : IJsErrorHandler, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IErrorLoggingService _errorLoggingService;
    private readonly ILogger<JsErrorHandler> _logger;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<JsErrorHandler>? _dotNetRef;

    public JsErrorHandler(IJSRuntime jsRuntime, IErrorLoggingService errorLoggingService, ILogger<JsErrorHandler> logger)
    {
        _jsRuntime = jsRuntime;
        _errorLoggingService = errorLoggingService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Check if we're in a context where JS interop is available
            if (!_jsRuntime.IsInitialized())
            {
                // Skip initialization during prerendering
                return;
            }
            
            // Only create these if they haven't been created yet
            if (_dotNetRef == null)
            {
                _dotNetRef = DotNetObjectReference.Create(this);
            }
            
            if (_jsModule == null)
            {
                _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./js/errorHandling.js");
                
                // Initialize the JavaScript error handler with a reference to this .NET object
                await _jsRuntime.InvokeVoidAsync("errorHandling.initialize", _dotNetRef);
                
                _logger.LogInformation("JavaScript error handler initialized");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize JavaScript error handler");
        }
    }

    [JSInvokable]
    public async Task HandleJavaScriptError(JsErrorInfo errorInfo)
    {
        await _errorLoggingService.LogJavaScriptErrorAsync(errorInfo);
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
