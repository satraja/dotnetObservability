# Loki Implementation Guide

## Overview

Loki is a log aggregation system designed by Grafana Labs that's specifically optimized for cloud-native applications. Unlike traditional log aggregation (like Elasticsearch), Loki doesn't index log contents but instead indexes labels, making it:

- **Lightweight** - Low memory/disk usage
- **Cost-effective** - Minimal operational overhead
- **Fast** - Quick log queries within label dimensions
- **Simple** - Works seamlessly with Grafana

Your application now logs to:
1. **Console** - For local development
2. **Loki** - For centralized log aggregation
3. **Both** - Simultaneously via Serilog

---

## Architecture

```
┌─────────────────────────────┐
│  Your .NET 10 Application   │
│  Serilog Logging            │
└──────────────┬──────────────┘
			   │
		┌──────┴──────┐
		│             │
   ┌────▼─────┐  ┌────▼─────┐
   │ Console  │  │   Loki   │
   └──────────┘  └────▼─────┘
					  │
					  │ HTTP Push
					  │
			  ┌───────▼────────┐
			  │ Loki Ingester  │
			  │ (Port 3100)    │
			  └───────┬────────┘
					  │
			  ┌───────▼────────┐
			  │  Loki Storage  │
			  │  (Filesystem)  │
			  └─────────────────┘
					  │
				 (Grafana reads)
					  │
			  ┌───────▼────────┐
			  │    Grafana     │
			  │  (Port 3000)   │
			  └────────────────┘
```

---

## What Was Added

### 1. **Serilog.Sinks.Loki Package**
- NuGet package that sends logs to Loki HTTP endpoint
- Batches logs for efficient transmission
- Configurable labels and batch sizes

### 2. **Program.cs Configuration**
```csharp
.WriteTo.LokiHttp(new LokiHttpClientOptions
{
	Uri = "http://localhost:3100",
	Labels = new[]
	{
		new LokiLabel { Key = "app", Value = "observability-app" },
		new LokiLabel { Key = "environment", Value = "Development" },
		new LokiLabel { Key = "version", Value = "1.0" }
	}
})
```

### 3. **Docker Compose Update**
- Added Loki service (port 3100)
- Configured Loki storage (filesystem-based)
- Added Grafana provisioning for auto-configuration

### 4. **Loki Config** (`loki-config.yml`)
- Ingestion settings
- Storage configuration
- Label handling

### 5. **Grafana Provisioning**
- Auto-configured Prometheus datasource
- Auto-configured Loki datasource
- Dashboard provisioning setup

---

## Starting the Stack

### 1. Build Your Application
```powershell
cd ObeservabiltyTraceabilty
dotnet build
```

### 2. Start Docker Containers
```powershell
docker-compose up -d
```

### 3. Verify Services
```bash
# Check Loki is running
curl http://localhost:3100/ready

# Check Prometheus is running
curl http://localhost:9090/-/healthy

# Check Grafana is running
curl http://localhost:3000/api/health
```

### 4. Access Services
- **Grafana**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090
- **Loki**: http://localhost:3100
- **PostgreSQL**: localhost:5432 (postgres/postgres)

---

## Running Your Application

### Option 1: From Visual Studio
1. Open the solution
2. Set up PostgreSQL connection string (or use default)
3. Press F5 to run
4. Application will start logging to both Console and Loki

### Option 2: From Command Line
```powershell
# Navigate to project
cd ObeservabiltyTraceabilty

# Run the application
dotnet run
```

---

## Using Loki in Grafana

### Step 1: Open Grafana
1. Navigate to http://localhost:3000
2. Login with admin/admin
3. Go to **Explore** (left sidebar)

### Step 2: Query Logs by Trace ID

**Example Query 1: Search by Trace ID**
```
{app="observability-app"} | json | TraceId="20240115120530-a1b2c3d4e5"
```

**Example Query 2: Search by Log Level**
```
{app="observability-app", level="error"}
```

**Example Query 3: Search with Regular Expression**
```
{app="observability-app"} | regexp "Error|Exception"
```

**Example Query 4: View All Logs**
```
{app="observability-app"}
```

### Step 3: Filter and Analyze
1. Select time range at top right
2. Use filters to narrow results
3. Click on log line to see full details
4. Use Statistics tab to see log distribution

---

## Log Labels (Searchable Dimensions)

Your logs are indexed with these labels:

| Label | Value | Purpose |
|-------|-------|---------|
| `app` | `observability-app` | Application name |
| `environment` | `Development` | Environment (dev/staging/prod) |
| `version` | `1.0` | Application version |

### Log Properties (In Structured Data)

Each log event also contains:
- `TraceId` - Request correlation ID
- `Timestamp` - When log occurred
- `Level` - Log level (Info, Warning, Error, etc.)
- `Message` - Log message
- `Exception` - Stack trace (if applicable)
- `Application` - Enriched property
- `Environment` - Enriched property

---

## Example: Troubleshooting with Trace ID

### Scenario: User reports error
1. Get trace ID from error response: `"traceId": "20240115120530-a1b2c3d4e5"`

### In Grafana:
1. Open **Explore** tab
2. Select **Loki** datasource (should be default)
3. Enter query:
   ```
   {app="observability-app"} | json | TraceId="20240115120530-a1b2c3d4e5"
   ```
4. Press **Run Query**
5. See all logs for that request in sequence:
   ```
   [12:05:30] GET /api/orders started
   [12:05:30] Connected to database
   [12:05:30] Retrieved 5 orders
   [12:05:30] Request completed - 150ms
   ```

---

## Query Language (LogQL)

Loki uses **LogQL** for querying. Here are common patterns:

### Basic Queries
```
# All logs from app
{app="observability-app"}

# Logs from development
{environment="Development"}

# Multiple labels (AND)
{app="observability-app", environment="Development"}
```

### Filter Expressions
```
# Logs containing "error" (case-insensitive)
{app="observability-app"} |= "error"

# Logs NOT containing "debug"
{app="observability-app"} != "debug"

# Regex match
{app="observability-app"} |~ "ERROR|Exception"

# JSON parsing then filter
{app="observability-app"} | json | Level="Error"
```

### Metrics from Logs
```
# Count of errors per minute
rate(({app="observability-app"} |= "error")[5m])

# Bytes over 5 minutes
bytes_over_time(({app="observability-app"})[5m])

# Line count rate
rate(({app="observability-app"})[5m])
```

### Common Queries

**Find all errors:**
```
{app="observability-app"} |= "error" or |= "Error" or |= "ERROR"
```

**Find slow requests (>1000ms):**
```
{app="observability-app"} | json | duration > 1000
```

**Find requests by controller:**
```
{app="observability-app"} |= "OrdersController"
```

**Find by trace ID pattern:**
```
{app="observability-app"} | json | TraceId=~".*20240115.*"
```

---

## Integration with Prometheus

Loki and Prometheus work together:

| Tool | Purpose | Stores |
|------|---------|--------|
| **Prometheus** | Performance metrics | Time-series data |
| **Loki** | Log aggregation | Structured logs |
| **Grafana** | Visualization | Both |

### In Grafana Dashboard:
1. Use Prometheus for graphs/metrics
2. Use Loki for detailed log analysis
3. Correlate both via trace ID

Example dashboard:
- **Top**: Prometheus graph showing API response times
- **Bottom**: Loki logs for the same time range
- **Link**: Both show same trace ID

---

## Configuration Reference

### Serilog LokiHttp Options

**In Program.cs:**

```csharp
.WriteTo.LokiHttp(new LokiHttpClientOptions
{
	Uri = "http://localhost:3100",                    // Loki server URL

	Labels = new[]
	{
		new LokiLabel { Key = "app", Value = "observability-app" },
		new LokiLabel { Key = "environment", Value = builder.Environment.EnvironmentName },
		new LokiLabel { Key = "version", Value = "1.0" }
	},

	PostMinimumLogEventBatchSize = 10,               // Min logs before sending
	Period = TimeSpan.FromSeconds(5),                // Max wait before sending
})
```

### Customizing Labels

**Add more labels:**
```csharp
Labels = new[]
{
	new LokiLabel { Key = "service", Value = "orders-api" },
	new LokiLabel { Key = "team", Value = "backend" },
	new LokiLabel { Key = "region", Value = "us-east-1" }
}
```

**Dynamic labels from config:**
```csharp
var labels = new List<LokiLabel>
{
	new LokiLabel { Key = "app", Value = builder.Configuration["App:Name"] },
	new LokiLabel { Key = "version", Value = builder.Configuration["App:Version"] }
};
```

---

## Troubleshooting

### Logs Not Appearing in Loki

**Check 1: Loki is running**
```powershell
docker ps
# Should show observability-loki container
```

**Check 2: Loki is accessible**
```powershell
curl http://localhost:3100/ready
# Should return "ready"
```

**Check 3: Application logs show connectivity**
Look for errors in console output like:
```
[WARN] Loki HTTP sink: Connection refused
```

**Fix: Restart docker containers**
```powershell
docker-compose down
docker-compose up -d
```

### Slow Log Ingestion

**Adjust batch settings in Program.cs:**
```csharp
PostMinimumLogEventBatchSize = 5,      // Send more frequently
Period = TimeSpan.FromSeconds(2)       // Shorter wait time
```

### Out of Memory (Loki container)

**Check Loki container logs:**
```powershell
docker logs observability-loki
```

**Increase retention in loki-config.yml:**
```yaml
limits_config:
  ingestion_rate_mb: 5      # Reduce from 10
```

**Restart container:**
```powershell
docker-compose restart loki
```

---

## Next Steps

### Short-term
1. ✅ Start Docker containers (`docker-compose up -d`)
2. ✅ Run your application
3. ✅ Access logs in Grafana Explore
4. ✅ Test queries by trace ID

### Medium-term
1. Create Grafana dashboards
2. Add alerts based on error patterns
3. Export logs for compliance/audit
4. Integrate with existing monitoring

### Long-term
1. Set up log retention policies
2. Implement log-based metrics
3. Create alerting rules
4. Performance optimization

---

## Best Practices

### ✅ DO
- Use consistent label names
- Include trace ID in important logs
- Query by trace ID for debugging
- Set appropriate log levels
- Archive old logs regularly

### ❌ DON'T
- Log sensitive data (passwords, PII)
- Use too many unique label values (causes cardinality explosion)
- Query without filtering by labels (slow)
- Keep infinite log retention
- Log at DEBUG level in production

---

## Performance Tips

### For Development
```csharp
PostMinimumLogEventBatchSize = 1,     // Send immediately (debug)
Period = TimeSpan.FromMilliseconds(500)
```

### For Production
```csharp
PostMinimumLogEventBatchSize = 50,    // Batch larger amounts
Period = TimeSpan.FromSeconds(10)     // Wait longer (more efficient)
```

### For High-Volume Logging
```csharp
.MinimumLevel.Warning()               // Log less
.WriteTo.Async(a => a.LokiHttp(...))  // Async batching
```

---

## Monitoring Loki Itself

**Check Loki health:**
```bash
curl http://localhost:3100/ready
curl http://localhost:3100/api/prom/label
```

**View Loki metrics (for Prometheus):**
```bash
curl http://localhost:3100/metrics
```

**Loki metrics to watch:**
- `loki_distributor_messages_received_total` - Incoming logs
- `loki_ingesters_queried` - Query load
- `loki_chunk_store_encoding_duration_seconds` - Performance

---

## Files Added/Modified

```
ObeservabiltyTraceabilty/
├── Program.cs ✓ (Modified - added Loki sink)
├── ObeservabiltyTraceabilty.csproj ✓ (Modified - added Serilog.Sinks.Loki)
├── docker-compose.yml ✓ (Modified - added Loki & improved Grafana)
├── loki-config.yml ✓ (New - Loki configuration)
├── grafana-provisioning/
│   ├── datasources/
│   │   └── datasources.yml ✓ (New - auto-configure Prometheus & Loki)
│   └── dashboards/
│       └── dashboards.yml ✓ (New - dashboard provisioning)
└── LOKI_IMPLEMENTATION.md (This file)
```

---

## Summary

Your observability stack now includes:

✅ **Metrics** via Prometheus  
✅ **Logs** via Loki  
✅ **Tracing** via Trace IDs  
✅ **Visualization** via Grafana  

All three pillars of observability working together! 🚀

Start using Loki to query your logs and correlate them with metrics and trace IDs!
