# Trace ID / Correlation ID Implementation

## Overview

Trace IDs (also known as Correlation IDs or Request IDs) are unique identifiers assigned to each HTTP request. They enable you to track a specific request throughout your entire application and across multiple services.

## Features Implemented

### ✅ Automatic Trace ID Generation
- Every request automatically gets a unique trace ID
- Format: `yyyyMMddHHmmss-uniqueid` (32 characters)
- Trace IDs can be provided via request headers

### ✅ Trace ID Propagation
- Trace ID flows through entire request pipeline
- Available in HTTP context
- Included in all logs automatically
- Added to response headers

### ✅ Structured Logging
- All log entries automatically include trace ID
- Trace ID appears in log output with rich formatting
- Makes debugging and troubleshooting easier

### ✅ API Response Integration
- Trace ID returned in API responses
- Enables clients to correlate their requests

---

## How It Works

### 1. Middleware (TraceIdMiddleware.cs)
The middleware intercepts every HTTP request:

```csharp
// Automatically executes for every request
public async Task InvokeAsync(HttpContext context)
{
	// Get or create trace ID
	var traceId = GetOrCreateTraceId(context);

	// Store in HttpContext
	context.Items["TraceId"] = traceId;

	// Add to response headers
	context.Response.Headers["X-Trace-Id"] = traceId;

	// Process request
	await _next(context);
}
```

### 2. Trace ID Sources
Trace ID is obtained in this priority order:

1. **X-Trace-Id Header** - If provided by client
2. **X-Correlation-Id Header** - If provided by client
3. **Auto-generated** - If not provided

### 3. Logging Enrichment
Serilog enricher automatically adds trace ID to every log statement:

```csharp
public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
{
	var traceId = httpContext.GetTraceId();
	var traceIdProperty = propertyFactory.CreateProperty("TraceId", traceId);
	logEvent.AddPropertyIfAbsent(traceIdProperty);
}
```

---

## Usage Guide

### Accessing Trace ID in Controllers

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetOrder(int id)
{
	// Get trace ID from HttpContext
	var traceId = HttpContext.GetTraceId();

	// Use in logging
	_logger.LogInformation("Processing order {Id} with TraceId: {TraceId}", id, traceId);

	// Return in response
	return Ok(new { data = order, traceId = traceId });
}
```

### Extension Methods

**Get Trace ID:**
```csharp
string traceId = HttpContext.GetTraceId();
// Returns: "20240115120530-a1b2c3d4e5"
```

**Get Correlation ID (alias):**
```csharp
string correlationId = HttpContext.GetCorrelationId();
// Same as GetTraceId()
```

**Set Trace ID:**
```csharp
HttpContext.SetTraceId("custom-trace-id-value");
```

### In Logging

All logs automatically include trace ID:

```csharp
_logger.LogInformation("User logged in");
// Output: [2024-01-15 12:05:30 INF] TraceId: 20240115120530-a1b2c3d4e5 User logged in
```

The trace ID is **automatically enriched** by the TraceIdEnricher - no need to manually add it!

---

## API Response Headers

Every API response includes trace ID headers:

```http
HTTP/1.1 200 OK
X-Trace-Id: 20240115120530-a1b2c3d4e5
X-Correlation-Id: 20240115120530-a1b2c3d4e5
Content-Type: application/json

{
  "success": true,
  "data": { ... },
  "traceId": "20240115120530-a1b2c3d4e5"
}
```

---

## Client Usage Examples

### Providing Trace ID in Request

**Using curl:**
```bash
curl -H "X-Trace-Id: my-custom-trace-id" \
	 https://localhost:7000/api/orders
```

**Using PowerShell:**
```powershell
$headers = @{
	"X-Trace-Id" = "my-custom-trace-id"
}
$response = Invoke-WebRequest `
	-Uri "https://localhost:7000/api/orders" `
	-Headers $headers `
	-SkipCertificateCheck
```

**Using C# HttpClient:**
```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-Trace-Id", "my-custom-trace-id");
var response = await client.GetAsync("https://localhost:7000/api/orders");
```

### Tracking Response Trace ID

```powershell
$response = Invoke-WebRequest `
	-Uri "https://localhost:7000/api/orders" `
	-SkipCertificateCheck

# Get trace ID from response header
$traceId = $response.Headers["X-Trace-Id"]
Write-Host "Response Trace ID: $traceId"

# Parse JSON response
$data = $response.Content | ConvertFrom-Json
Write-Host "Response Trace ID (in body): $($data.traceId)"
```

---

## Finding Logs by Trace ID

All logs for a request can be found by searching for trace ID:

### Using Log Aggregation (ELK, Grafana, etc.)
```
TraceId: 20240115120530-a1b2c3d4e5
```

### Console Output
```
[2024-01-15 12:05:30 INF] TraceId: 20240115120530-a1b2c3d4e5 GetAllOrders started
[2024-01-15 12:05:30 INF] TraceId: 20240115120530-a1b2c3d4e5 Retrieved 5 orders
```

---

## Trace ID Flow Example

### Request Flow with Trace ID

```
1. Client Request
   ↓
   curl -H "X-Trace-Id: abc123" https://localhost:7000/api/orders

2. TraceIdMiddleware (First in pipeline)
   ↓
   - Reads X-Trace-Id header: "abc123"
   - Stores in HttpContext.Items["TraceId"]: "abc123"
   - Adds to response headers

3. Controller
   ↓
   OrdersController.GetAllOrders()
   - Gets traceId = HttpContext.GetTraceId() → "abc123"
   - Logs: "Retrieved 5 orders - TraceId: abc123"

4. Service Layer
   ↓
   OrderService.GetAllOrdersAsync()
   - Logs use enriched tracer (automatic)
   - Log: "TraceId: abc123 Fetching orders from database"

5. Response
   ↓
   HTTP/1.1 200 OK
   X-Trace-Id: abc123
   { success: true, data: [...], traceId: "abc123" }

6. Output Logs (all linked by TraceId: abc123)
   [12:05:30 INF] TraceId: abc123 GetAllOrders started
   [12:05:30 INF] TraceId: abc123 Fetching orders from database
   [12:05:30 INF] TraceId: abc123 Retrieved 5 orders
```

---

## Troubleshooting

### Trace ID Not Appearing in Logs

**Check:**
1. Middleware is registered: `app.UseMiddleware<TraceIdMiddleware>();`
2. Serilog enricher is configured with trace ID enrichment
3. Log output template includes `{TraceId}`: 
   ```
   "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] TraceId: {TraceId} {Message:lj}"
   ```

### Can't Get Trace ID in Controller

**Check:**
1. Using extension method: `HttpContext.GetTraceId()`
2. HttpContext is not null
3. Middleware is running (check middleware registration order)

### Trace ID Not in Response Headers

**Check:**
1. Response hasn't been sent yet when setting headers
2. Headers set before middleware finishes
3. Response compression not interfering

---

## Best Practices

### ✅ DO

- **Always include trace ID in log messages when debugging**
  ```csharp
  _logger.LogInformation("Processing order {Id} with TraceId: {TraceId}", id, traceId);
  ```

- **Return trace ID in error responses**
  ```csharp
  return StatusCode(500, new { error = "Something went wrong", traceId = traceId });
  ```

- **Use trace ID in client error reporting**
  ```
  "If this error persists, please reference trace ID: {traceId} when contacting support"
  ```

- **Implement log aggregation to search by trace ID**

### ❌ DON'T

- **Modify trace ID mid-request** (unless absolutely necessary)
- **Log sensitive data with trace ID**
- **Use trace ID for security purposes** (use proper auth instead)
- **Assume trace ID is cryptographically secure** (it's not for security)

---

## Integration with External Systems

### Sending Trace ID to External APIs

When calling external services, propagate trace ID:

```csharp
var client = new HttpClient();
var traceId = HttpContext.GetTraceId();

// Pass trace ID to external API
client.DefaultRequestHeaders.Add("X-Trace-Id", traceId);
var response = await client.GetAsync("https://external-api.com/data");
```

### Receiving Trace ID from External APIs

Handle trace IDs from external services:

```csharp
var externalTraceId = response.Headers.FirstOrDefault("X-Trace-Id").Value ?? "N/A";
_logger.LogInformation(
	"External API call completed - DataTraceId: {TraceId}, ExternalTraceId: {ExternalTraceId}",
	traceId, externalTraceId
);
```

---

## Monitoring & Observability

### Trace ID in Prometheus Metrics

While metrics themselves don't include trace ID (that's what logs are for), you can use trace ID to correlate:

```
1. Get trace ID from API response
2. Search logs by trace ID
3. Correlate with metrics time range
4. Trace entire request lifecycle
```

### Grafana Log Integration

If using Grafana with Loki or similar:

1. Setup log aggregation
2. Query by trace ID:
   ```
   {traceId="20240115120530-a1b2c3d4e5"}
   ```
3. View all logs for that request in sequence

---

## Trace ID Format

Current format: `yyyyMMddHHmmss-randomstring` (32 chars total)

Example: `20240115120530-a1b2c3d4e5f1g2h3`

**Breakdown:**
- `20240115120530` - Timestamp (YYYY-MM-DD HH:MM:SS)
- `-a1b2c3d4e5f1g2h3` - Random GUID portion (16 chars)
- **Total: 32 characters** (URL-safe, sortable, unique)

---

## Configuration

### Customizing Trace ID Header Names

Edit `TraceIdMiddleware.cs`:

```csharp
private const string TraceIdHeaderName = "X-Trace-Id";        // Customize here
private const string CorrelationIdHeaderName = "X-Correlation-Id";  // Or here
```

### Customizing Trace ID Format

Edit `TraceIdMiddleware.cs` in `GetOrCreateTraceId()`:

```csharp
// Current format
return $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".Substring(0, 32);

// Custom format example
return $"APP-{Guid.NewGuid():D}";  // Format: APP-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

---

## Summary

Trace IDs provide **end-to-end request tracking** enabling you to:

✅ Correlate logs across services
✅ Debug issues quickly
✅ Track request lifecycle
✅ Support customer debugging
✅ Monitor system health
✅ Performance analysis

All implemented automatically with minimal code! 🚀
