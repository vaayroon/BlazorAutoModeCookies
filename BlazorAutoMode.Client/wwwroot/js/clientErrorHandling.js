// Client-side JavaScript error handling (WebAssembly-specific)
export const clientErrorHandling = {
    initialize: function (dotNetReference) {
        // Store the .NET reference for later use
        this.dotNetReference = dotNetReference;
        
        // Capture unhandled errors in the WebAssembly context
        window.addEventListener('unhandledrejection', (event) => {
            this.handleError(
                event.reason?.message || "Unhandled Promise Rejection in WebAssembly",
                "WebAssembly Promise",
                0,
                0,
                event.reason?.stack || ""
            );
        });
        
        // Capture Blazor WebAssembly-specific errors
        window.addEventListener('blazor:webassembly:error', (event) => {
            const detail = event.detail || {};
            this.handleError(
                detail.message || "Blazor WebAssembly Error",
                detail.source || "Blazor WebAssembly Runtime",
                detail.lineno || 0,
                detail.colno || 0,
                detail.stack || ""
            );
        });
        
        console.log("Client-side error handling initialized");
    },
    
    handleError: function (message, source, line, column, stack) {
        console.error(`WebAssembly Error: ${message}\nSource: ${source}\nLine: ${line}, Column: ${column}\nStack: ${stack}`);
        
        if (this.dotNetReference) {
            try {
                this.dotNetReference.invokeMethodAsync('HandleJavaScriptErrorAsync', 
                    message, source, line, column, stack);
            } catch (err) {
                console.error("Failed to send error to .NET", err);
            }
        }
    }
};
