// Browser diagnostics module for capturing console logs and network activity
// Implements the Browser Console and Network Capture functionality

let dotNetObjectRef = null;
let originalConsole = {};
let networkInterceptor = null;

export function initializeDiagnostics(dotNetRef) {
    dotNetObjectRef = dotNetRef;
    interceptConsole();
    // interceptNetworkRequests(); // Disabled to reduce log noise
    interceptUserActions();
    
    console.info('[Diagnostics] Browser diagnostics initialized successfully (network interception disabled)');
}

function interceptConsole() {
    // Store original console methods
    originalConsole = {
        log: console.log,
        info: console.info,
        warn: console.warn,
        error: console.error,
        debug: console.debug
    };

    // Override console methods
    ['log', 'info', 'warn', 'error', 'debug'].forEach(level => {
        console[level] = function(...args) {
            // Call original method
            originalConsole[level].apply(console, args);
            
            // Send to server
            if (dotNetObjectRef) {
                const message = args.map(arg => 
                    typeof arg === 'object' ? JSON.stringify(arg) : String(arg)
                ).join(' ');
                
                const stack = level === 'error' && args[0] instanceof Error ? args[0].stack : null;
                
                dotNetObjectRef.invokeMethodAsync('LogConsoleMessage', 
                    level, 
                    message, 
                    new Date().toISOString(),
                    stack
                ).catch(() => {
                    // Silently ignore errors to prevent infinite loops
                });
            }
        };
    });

    // Capture unhandled errors
    window.addEventListener('error', (event) => {
        if (dotNetObjectRef) {
            dotNetObjectRef.invokeMethodAsync('LogConsoleMessage',
                'error',
                `Unhandled error: ${event.message} at ${event.filename}:${event.lineno}:${event.colno}`,
                new Date().toISOString(),
                event.error ? event.error.stack : null
            ).catch(() => {});
        }
    });

    // Capture unhandled promise rejections
    window.addEventListener('unhandledrejection', (event) => {
        if (dotNetObjectRef) {
            dotNetObjectRef.invokeMethodAsync('LogConsoleMessage',
                'error',
                `Unhandled promise rejection: ${event.reason}`,
                new Date().toISOString(),
                event.reason && event.reason.stack ? event.reason.stack : null
            ).catch(() => {});
        }
    });
}

function interceptNetworkRequests() {
    // DISABLED: Network request interception temporarily disabled to reduce log noise
    // The network activity logging was causing cascading diagnostic requests
    // that flood the console with verbose ASP.NET Core request pipeline logs
    console.info('[Diagnostics] Network request interception disabled to reduce log noise');
    return;
    window.fetch = function(...args) {
        const startTime = performance.now();
        const url = typeof args[0] === 'string' ? args[0] : args[0].url;
        const method = args[1] ? (args[1].method || 'GET') : 'GET';
        const requestId = generateRequestId();

        // Skip logging for diagnostic endpoints to prevent infinite loops
        if (shouldExcludeUrl(url)) {
            return originalFetch.apply(this, args);
        }

        return originalFetch.apply(this, args)
            .then(response => {
                const duration = performance.now() - startTime;
                
                if (dotNetObjectRef) {
                    dotNetObjectRef.invokeMethodAsync('LogNetworkActivity',
                        method,
                        url,
                        response.status,
                        duration,
                        requestId
                    ).catch(() => {});
                }
                
                return response;
            })
            .catch(error => {
                const duration = performance.now() - startTime;
                
                if (dotNetObjectRef) {
                    dotNetObjectRef.invokeMethodAsync('LogNetworkActivity',
                        method,
                        url,
                        0, // Status 0 for network errors
                        duration,
                        requestId
                    ).catch(() => {});
                }
                
                throw error;
            });
    };

    // Intercept XMLHttpRequest
    const originalXHROpen = XMLHttpRequest.prototype.open;
    const originalXHRSend = XMLHttpRequest.prototype.send;
    
    XMLHttpRequest.prototype.open = function(method, url, ...args) {
        this._method = method;
        this._url = url;
        this._startTime = performance.now();
        this._requestId = generateRequestId();
        this._shouldExclude = shouldExcludeUrl(url);
        
        return originalXHROpen.call(this, method, url, ...args);
    };
    
    XMLHttpRequest.prototype.send = function(...args) {
        const xhr = this;
        
        // Only add event listener if we're not excluding this URL
        if (!xhr._shouldExclude) {
            xhr.addEventListener('loadend', () => {
                const duration = performance.now() - xhr._startTime;
                
                if (dotNetObjectRef) {
                    dotNetObjectRef.invokeMethodAsync('LogNetworkActivity',
                        xhr._method || 'GET',
                        xhr._url || '',
                        xhr.status,
                        duration,
                        xhr._requestId || generateRequestId()
                    ).catch(() => {});
                }
            });
        }
        
        return originalXHRSend.apply(this, args);
    };
}

function interceptUserActions() {
    // Track clicks
    document.addEventListener('click', (event) => {
        if (dotNetObjectRef && event.target) {
            const elementType = event.target.tagName.toLowerCase();
            const elementId = event.target.id || null;
            const data = {
                className: event.target.className,
                text: event.target.textContent ? event.target.textContent.substring(0, 100) : null,
                coordinates: { x: event.clientX, y: event.clientY }
            };
            
            dotNetObjectRef.invokeMethodAsync('LogUserAction',
                'click',
                elementType,
                elementId,
                JSON.stringify(data)
            ).catch(() => {});
        }
    });

    // Track form submissions
    document.addEventListener('submit', (event) => {
        if (dotNetObjectRef && event.target) {
            const formId = event.target.id || null;
            const data = {
                action: event.target.action,
                method: event.target.method,
                fieldCount: event.target.elements.length
            };
            
            dotNetObjectRef.invokeMethodAsync('LogUserAction',
                'form-submit',
                'form',
                formId,
                JSON.stringify(data)
            ).catch(() => {});
        }
    });

    // Track input changes (debounced)
    let inputTimeout;
    document.addEventListener('input', (event) => {
        if (dotNetObjectRef && event.target && event.target.tagName) {
            clearTimeout(inputTimeout);
            inputTimeout = setTimeout(() => {
                const elementType = event.target.tagName.toLowerCase();
                const elementId = event.target.id || null;
                const data = {
                    type: event.target.type,
                    name: event.target.name,
                    valueLength: event.target.value ? event.target.value.length : 0
                };
                
                dotNetObjectRef.invokeMethodAsync('LogUserAction',
                    'input-change',
                    elementType,
                    elementId,
                    JSON.stringify(data)
                ).catch(() => {});
            }, 500);
        }
    });
}

function generateRequestId() {
    return Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
}

export function cleanup() {
    // Restore original console methods
    if (originalConsole.log) {
        console.log = originalConsole.log;
        console.info = originalConsole.info;
        console.warn = originalConsole.warn;
        console.error = originalConsole.error;
        console.debug = originalConsole.debug;
    }
    
    dotNetObjectRef = null;
    
    console.info('[Diagnostics] Browser diagnostics cleaned up');
}
