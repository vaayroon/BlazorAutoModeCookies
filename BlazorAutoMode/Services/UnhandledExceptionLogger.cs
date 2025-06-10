using System;
using Microsoft.Extensions.Logging;

namespace BlazorAutoMode.Services;

public interface IUnhandledExceptionLogger
{
    void LogUnhandledException(Exception exception, string source, string? additionalInfo = null);
}

public class UnhandledExceptionLogger : IUnhandledExceptionLogger
{
    private readonly ILogger<UnhandledExceptionLogger> _logger;

    public UnhandledExceptionLogger(ILogger<UnhandledExceptionLogger> logger)
    {
        _logger = logger;
    }

    public void LogUnhandledException(Exception exception, string source, string? additionalInfo = null)
    {
        _logger.LogError(
            exception,
            "Error occurred in {Source}. {AdditionalInfo}",
            source ?? "Unknown source",
            additionalInfo ?? string.Empty
        );
    }
}
