// Global JavaScript error handling
window.errorHandling = {
    initialize: function (dotNetReference) {
        // Store the .NET reference for later use
        this.dotNetReference = dotNetReference;
        
        // Handle runtime errors
        window.onerror = (message, source, lineno, colno, error) => {
            this.handleError(message, source, lineno, colno, error?.stack);
            // Return false to let the browser handle the error as well
            return false;
        };
        
        // Handle promise rejections
        window.onunhandledrejection = (event) => {
            const message = event.reason?.message || "Unhandled Promise Rejection";
            const stack = event.reason?.stack || "";
            this.handleError(message, "Promise Rejection", 0, 0, stack);
        };
        
        // Handle Blazor-specific errors
        window.addEventListener('blazor:error', (event) => {
            const detail = event.detail || {};
            this.handleError(
                detail.message || "Blazor Error", 
                detail.source || "Blazor Runtime",
                detail.lineno || 0, 
                detail.colno || 0, 
                detail.stack || ""
            );
        });
        
        console.log("JavaScript error handling initialized");
    },
    
    handleError: function (message, source, lineno, colno, stack) {
        // Log the error to the console
        console.error(`Error: ${message}\nSource: ${source}\nLine: ${lineno}, Column: ${colno}\nStack: ${stack}`);
        
        // Send the error to the .NET side if reference is available
        if (this.dotNetReference) {
            try {
                this.dotNetReference.invokeMethodAsync('HandleJavaScriptError', {
                    message: message,
                    source: source,
                    lineNumber: lineno,
                    columnNumber: colno,
                    stack: stack,
                    timestamp: new Date().toISOString()
                });
            } catch (err) {
                console.error("Failed to send error to .NET", err);
            }
        }
    }
};
