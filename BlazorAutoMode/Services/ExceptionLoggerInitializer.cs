using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorAutoMode.Services;

public class ExceptionLoggerInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExceptionLoggerInitializer> _logger;

    public ExceptionLoggerInitializer(
        IServiceProvider serviceProvider,
        ILogger<ExceptionLoggerInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing exception handlers");
        
        // Create a static exception handler for AppDomain
        StaticExceptionHandler.Initialize(_serviceProvider, _logger);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

// This static class will hold the AppDomain event handlers
public static class StaticExceptionHandler
{
    private static IServiceProvider _serviceProvider = null!;
    private static ILogger _logger = null!;

    public static void Initialize(IServiceProvider serviceProvider, ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Subscribe to the AppDomain unhandled exception events
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        
        logger.LogInformation("Static exception handler initialized");
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var errorLoggingService = scope.ServiceProvider.GetRequiredService<IErrorLoggingService>();
            
            if (e.ExceptionObject is Exception exception)
            {
                // Log directly using the error logging service
                errorLoggingService.LogError(
                    exception, 
                    "AppDomain.UnhandledException", 
                    $"IsTerminating: {e.IsTerminating}"
                );
            }
            else
            {
                errorLoggingService.LogError(
                    $"Non-exception object in UnhandledException event: {e.ExceptionObject}",
                    "AppDomain.UnhandledException",
                    $"IsTerminating: {e.IsTerminating}"
                );
            }
        }
        catch (Exception ex)
        {
            // Fallback to the static logger if there's an issue creating the scope
            _logger.LogError(ex, "Error while handling unhandled exception");
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var errorLoggingService = scope.ServiceProvider.GetRequiredService<IErrorLoggingService>();
            
            errorLoggingService.LogError(
                e.Exception,
                "TaskScheduler.UnobservedTaskException",
                "This exception was not observed in a task"
            );
            
            // Mark as observed to prevent the application from crashing
            e.SetObserved();
        }
        catch (Exception ex)
        {
            // Fallback to the static logger if there's an issue creating the scope
            _logger.LogError(ex, "Error while handling unobserved task exception");
            
            // Still mark as observed
            e.SetObserved();
        }
    }
}
