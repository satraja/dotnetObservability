using ObeservabiltyTraceabilty.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace ObeservabiltyTraceabilty.Logging;

/// <summary>
/// Serilog enricher to add trace ID to all log events
/// </summary>
public class TraceIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TraceIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
            return;

        var traceId = httpContext.GetTraceId();

        if (!string.IsNullOrWhiteSpace(traceId))
        {
            var traceIdProperty = propertyFactory.CreateProperty("CorrelationId", traceId);
            logEvent.AddOrUpdateProperty(traceIdProperty);
        }

        // Also add request path and method
        var requestInfo = $"{httpContext.Request.Method} {httpContext.Request.Path}";
        var requestProperty = propertyFactory.CreateProperty("RequestInfo", requestInfo);
        logEvent.AddPropertyIfAbsent(requestProperty);
    }
}
