using System;
using Microsoft.Extensions.Logging;

namespace BlazorAutoMode.Services;

public interface IUnhandledExceptionLogger
{
    void Initialize();
}

public class UnhandledExceptionLogger : IUnhandledExceptionLogger
{
    private readonly ILogger<UnhandledExceptionLogger> _logger;
    private readonly IErrorLoggingService _errorLoggingService;

    public UnhandledExceptionLogger(ILogger<UnhandledExceptionLogger> logger, IErrorLoggingService errorLoggingService)
    {
        _logger = logger;
        _errorLoggingService = errorLoggingService;
    }

    public void Initialize()
    {
        // Subscribe to the unhandled exception event
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        // Subscribe to task scheduler unobserved task exceptions
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        
        _logger.LogInformation("Unhandled exception logger initialized");
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _errorLoggingService.LogError(
                exception, 
                "AppDomain.UnhandledException", 
                $"IsTerminating: {e.IsTerminating}"
            );
        }
        else
        {
            _errorLoggingService.LogError(
                $"Non-exception object in UnhandledException event: {e.ExceptionObject}",
                "AppDomain.UnhandledException",
                $"IsTerminating: {e.IsTerminating}"
            );
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _errorLoggingService.LogError(
            e.Exception,
            "TaskScheduler.UnobservedTaskException",
            "This exception was not observed in a task"
        );
        
        // Mark as observed to prevent the application from crashing
        e.SetObserved();
    }
}
