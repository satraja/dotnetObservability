namespace ObeservabiltyTraceabilty.DTOs;

/// <summary>
/// Base response wrapper that includes trace ID for request tracking
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string? traceId = null, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation successful",
            TraceId = traceId
        };
    }

    public static ApiResponse<T> Error(string message, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            TraceId = traceId
        };
    }
}

/// <summary>
/// Error response with trace ID
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Pagination metadata with trace ID
/// </summary>
public class PaginatedResponse<T>
{
    public bool Success { get; set; }
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
