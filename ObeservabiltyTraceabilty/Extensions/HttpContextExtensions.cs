namespace ObeservabiltyTraceabilty.Extensions;

/// <summary>
/// Extension methods for HttpContext trace ID access
/// </summary>
public static class HttpContextExtensions
{
    private const string TraceIdKey = "TraceId";
    private const string CorrelationIdKey = "CorrelationId";

    /// <summary>
    /// Get the trace ID for the current request
    /// </summary>
    public static string GetTraceId(this HttpContext context)
    {
        if (context == null)
            return "unknown";

        if (context.Items.TryGetValue(TraceIdKey, out var traceId) && traceId is string traceIdString)
        {
            return traceIdString;
        }

        return "unknown";
    }

    /// <summary>
    /// Get the correlation ID for the current request
    /// </summary>
    public static string GetCorrelationId(this HttpContext context)
    {
        return context.GetTraceId();
    }

    /// <summary>
    /// Set the trace ID for the current request
    /// </summary>
    public static void SetTraceId(this HttpContext context, string traceId)
    {
        context.Items[TraceIdKey] = traceId;
        context.Items[CorrelationIdKey] = traceId;
    }
}
