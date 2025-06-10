using System;
using Microsoft.JSInterop;

namespace BlazorAutoMode.Services;

public static class JSRuntimeExtensions
{
    public static bool IsInitialized(this IJSRuntime jsRuntime)
    {
        try
        {
            // Attempt to access a property that would throw an exception if JS interop is not available
            _ = jsRuntime.ToString();
            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            // If we get a specific exception about JS interop not being available, return false
            return false;
        }
        catch
        {
            // For any other exception, assume JS interop is available
            return true;
        }
    }
}
