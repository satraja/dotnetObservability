namespace ObeservabiltyTraceabilty.Middleware;

/// <summary>
/// Middleware for handling correlation/trace IDs for request tracking
/// </summary>
public class TraceIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TraceIdMiddleware> _logger;
    private const string TraceIdHeaderName = "X-Trace-Id";
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    public TraceIdMiddleware(RequestDelegate next, ILogger<TraceIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or create trace ID
        var traceId = GetOrCreateTraceId(context);

        // Add trace ID to HttpContext for use throughout the request
        context.Items["TraceId"] = traceId;
        context.Items["CorrelationId"] = traceId;

        // Add trace ID to response headers
        context.Response.Headers[TraceIdHeaderName] = traceId;
        context.Response.Headers[CorrelationIdHeaderName] = traceId;

        _logger.LogInformation("Request started with TraceId: {TraceId}", traceId);

        try
        {
            await _next(context);
        }
        finally
        {
            _logger.LogInformation("Request completed with TraceId: {TraceId} - StatusCode: {StatusCode}", traceId, context.Response.StatusCode);
        }
    }

    private string GetOrCreateTraceId(HttpContext context)
    {
        // Check if trace ID is provided in request headers
        if (context.Request.Headers.TryGetValue(TraceIdHeaderName, out var traceIdHeader) && 
            !string.IsNullOrWhiteSpace(traceIdHeader))
        {
            return traceIdHeader.ToString();
        }

        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdHeader) && 
            !string.IsNullOrWhiteSpace(correlationIdHeader))
        {
            return correlationIdHeader.ToString();
        }

        // Generate new trace ID if not provided
        return $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".Substring(0, 32);
    }
}
