# Trace ID Testing Guide

Quick reference for testing trace ID functionality.

## Test 1: Auto-Generated Trace ID

**Request:**
```bash
curl -X GET https://localhost:7000/api/orders \
  -H "Accept: application/json" \
  -k
```

**Expected Response:**
```json
{
  "success": true,
  "data": [...],
  "traceId": "20240115120530-a1b2c3d4e5f1g2h3",
  "count": 5
}
```

**Response Headers:**
```
X-Trace-Id: 20240115120530-a1b2c3d4e5f1g2h3
X-Correlation-Id: 20240115120530-a1b2c3d4e5f1g2h3
```

---

## Test 2: Custom Trace ID from Header

**Request:**
```bash
curl -X GET https://localhost:7000/api/orders \
  -H "Accept: application/json" \
  -H "X-Trace-Id: my-custom-id-123" \
  -k
```

**Expected Response:**
```json
{
  "success": true,
  "data": [...],
  "traceId": "my-custom-id-123",
  "count": 5
}
```

**Response Headers:**
```
X-Trace-Id: my-custom-id-123
X-Correlation-Id: my-custom-id-123
```

---

## Test 3: Correlation ID Alternative Header

**Request:**
```bash
curl -X GET https://localhost:7000/api/orders \
  -H "Accept: application/json" \
  -H "X-Correlation-Id: correlation-id-456" \
  -k
```

**Expected Response:**
```json
{
  "success": true,
  "data": [...],
  "traceId": "correlation-id-456",
  "count": 5
}
```

---

## Test 4: Verify Logs Include Trace ID

**Setup:**
1. Start application
2. Make request with trace ID: `X-Trace-Id: test-trace-123`
3. Check application console output

**Expected Console Output:**
```
[2024-01-15 12:05:30 INF] TraceId: test-trace-123 GetAllOrders started - TraceId: test-trace-123
[2024-01-15 12:05:30 INF] TraceId: test-trace-123 Retrieved 5 orders - TraceId: test-trace-123
```

---

## Test 5: Error Response with Trace ID

**Request (invalid order ID):**
```bash
curl -X GET https://localhost:7000/api/orders/99999 \
  -H "X-Trace-Id: error-test-789" \
  -k
```

**Expected Response (404):**
```json
{
  "success": false,
  "message": "Order not found",
  "traceId": "error-test-789"
}
```

---

## PowerShell Testing Script

**Save as `Test-TraceIds.ps1`:**

```powershell
$baseUrl = "https://localhost:7000/api"
$customTraceId = "ps-test-$(Get-Random)"

Write-Host "Testing Trace ID Implementation" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Auto-generated trace ID
Write-Host "Test 1: Auto-generated Trace ID" -ForegroundColor Yellow
$response1 = Invoke-WebRequest -Uri "$baseUrl/orders" -SkipCertificateCheck
$data1 = $response1.Content | ConvertFrom-Json
$traceId1 = $response1.Headers["X-Trace-Id"]
Write-Host "  Trace ID from header: $traceId1" -ForegroundColor Green
Write-Host "  Trace ID from response: $($data1.traceId)" -ForegroundColor Green
Write-Host ""

# Test 2: Custom trace ID
Write-Host "Test 2: Custom Trace ID ($customTraceId)" -ForegroundColor Yellow
$headers = @{ "X-Trace-Id" = $customTraceId }
$response2 = Invoke-WebRequest -Uri "$baseUrl/orders" -Headers $headers -SkipCertificateCheck
$data2 = $response2.Content | ConvertFrom-Json
$traceId2 = $response2.Headers["X-Trace-Id"]
Write-Host "  Trace ID from header: $traceId2" -ForegroundColor Green
Write-Host "  Trace ID from response: $($data2.traceId)" -ForegroundColor Green
Write-Host "  Match custom ID: $(if ($traceId2 -eq $customTraceId) { 'YES ✓' } else { 'NO ✗' })" -ForegroundColor $(if ($traceId2 -eq $customTraceId) { 'Green' } else { 'Red' })
Write-Host ""

# Test 3: Multiple endpoints same trace ID
Write-Host "Test 3: Propagating Trace ID to Multiple Endpoints" -ForegroundColor Yellow
$traceId3 = "multi-test-$(Get-Random)"
$headers3 = @{ "X-Trace-Id" = $traceId3 }

Write-Host "  GET /api/orders with traceId: $traceId3"
$r1 = Invoke-WebRequest -Uri "$baseUrl/orders" -Headers $headers3 -SkipCertificateCheck
$d1 = $r1.Content | ConvertFrom-Json
Write-Host "    Response TraceId: $($d1.traceId)" -ForegroundColor Green

Write-Host "  GET /api/services with same traceId"
$r2 = Invoke-WebRequest -Uri "$baseUrl/services" -Headers $headers3 -SkipCertificateCheck
$d2 = $r2.Content | ConvertFrom-Json
Write-Host "    Response TraceId: $($d2.traceId)" -ForegroundColor Green

Write-Host ""
Write-Host "✓ Trace ID testing complete!" -ForegroundColor Cyan
Write-Host ""
```

**Run it:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
.\Test-TraceIds.ps1
```

---

## Trace ID Benefits

| Scenario | Benefit |
|----------|---------|
| **User Reports Bug** | Provide Trace ID to get all logs for that request |
| **Multi-Service Calls** | Trace ID flows through entire chain |
| **Performance Analysis** | Correlate logs with metrics for same request |
| **Troubleshooting** | Search logs by Trace ID to find root cause |
| **Audit Trail** | Every request is tracked and traceable |

---

## Integration with Log Aggregation

### Elasticsearch/Kibana
```
query: traceId:"test-trace-123"
```

### Grafana/Loki
```
{traceId="test-trace-123"}
```

### Splunk
```
traceId=test-trace-123
```

### Azure Application Insights
```
customDimensions.TraceId == "test-trace-123"
```
