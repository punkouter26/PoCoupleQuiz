// Application Insights Telemetry Configuration for Blazor WebAssembly
// Ensures W3C Trace Context correlation between client and server

export function initializeAppInsights(connectionString) {
    if (!connectionString) {
        console.warn('[Telemetry] Application Insights connection string not configured');
        return false;
    }

    // Update the Application Insights configuration
    if (window.appInsights) {
        window.appInsights.config.connectionString = connectionString;
        
        // Add telemetry initializer to enrich all client telemetry
        window.appInsights.addTelemetryInitializer((envelope) => {
            // Add custom properties to all telemetry
            envelope.tags = envelope.tags || [];
            envelope.data = envelope.data || {};
            envelope.data.source = 'BlazorWASM';
            
            // Ensure W3C Trace Context is used for correlation
            // The SDK handles this automatically when enableCorsCorrelation is true
            return true;
        });

        console.info('[Telemetry] Application Insights initialized with connection string');
        return true;
    } else {
        console.error('[Telemetry] Application Insights SDK not loaded');
        return false;
    }
}

export function trackEvent(name, properties, measurements) {
    if (window.appInsights) {
        window.appInsights.trackEvent({
            name: name,
            properties: properties || {},
            measurements: measurements || {}
        });
    }
}

export function trackMetric(name, average, properties) {
    if (window.appInsights) {
        window.appInsights.trackMetric({
            name: name,
            average: average,
            properties: properties || {}
        });
    }
}

export function trackException(error, severityLevel, properties) {
    if (window.appInsights) {
        window.appInsights.trackException({
            exception: error,
            severityLevel: severityLevel || 3, // Error
            properties: properties || {}
        });
    }
}

export function trackPageView(name, url, properties) {
    if (window.appInsights) {
        window.appInsights.trackPageView({
            name: name || document.title,
            uri: url || window.location.href,
            properties: properties || {}
        });
    }
}

export function flush() {
    if (window.appInsights) {
        window.appInsights.flush();
    }
}
