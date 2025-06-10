using System;

namespace BlazorAutoMode.Services;

public class JsErrorInfo
{
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public int ColumnNumber { get; set; }
    public string? Stack { get; set; }
    public string Timestamp { get; set; } = string.Empty;
}
