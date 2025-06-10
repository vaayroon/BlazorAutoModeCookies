using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorAutoMode.Services;

public interface IErrorLoggingService
{
    void LogError(Exception exception, string? source = null, string? additionalInfo = null);
    void LogError(string message, string? source = null, string? additionalInfo = null);
    Task LogJavaScriptErrorAsync(JsErrorInfo errorInfo);
}

public class ErrorLoggingService : IErrorLoggingService
{
    private readonly ILogger<ErrorLoggingService> _logger;

    public ErrorLoggingService(ILogger<ErrorLoggingService> logger)
    {
        _logger = logger;
    }

    public void LogError(Exception exception, string? source = null, string? additionalInfo = null)
    {
        _logger.LogError(
            exception,
            "Error occurred in {Source}. {AdditionalInfo}",
            source ?? "Unknown source",
            additionalInfo ?? string.Empty
        );
    }

    public void LogError(string message, string? source = null, string? additionalInfo = null)
    {
        _logger.LogError(
            "Error message: {Message}. Source: {Source}. {AdditionalInfo}",
            message,
            source ?? "Unknown source",
            additionalInfo ?? string.Empty
        );
    }

    public Task LogJavaScriptErrorAsync(JsErrorInfo errorInfo)
    {
        _logger.LogError(
            "JavaScript error: {Message} at {Source}:{LineNumber}:{ColumnNumber}. Stack: {Stack}. Timestamp: {Timestamp}",
            errorInfo.Message,
            errorInfo.Source,
            errorInfo.LineNumber,
            errorInfo.ColumnNumber,
            errorInfo.Stack,
            errorInfo.Timestamp
        );
        
        return Task.CompletedTask;
    }
}
