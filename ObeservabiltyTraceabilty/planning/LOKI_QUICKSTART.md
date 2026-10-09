# Loki Quick Start Guide

## 🚀 Getting Started with Loki in 5 Minutes

### Prerequisites
- Docker and Docker Compose installed
- .NET 10 SDK
- Your observability application built

---

## Step 1: Start the Docker Stack

```powershell
# Navigate to project root
cd D:\workplace\dotnet\ObeservabiltyTraceabilty\

# Start all services (PostgreSQL, Prometheus, Loki, Grafana)
docker-compose up -d
```

**Verify services are running:**
```powershell
docker ps
```

You should see:
- `observability-postgres` - Database (port 5432)
- `observability-prometheus` - Metrics (port 9090)
- `observability-loki` - Logs (port 3100)
- `observability-grafana` - Dashboard (port 3000)

---

## Step 2: Run Your Application

### Option A: From Visual Studio
1. Open `ObeservabiltyTraceabilty.slnx`
2. Press F5 to run (Debug mode)
3. Application starts on https://localhost:7000

### Option B: From Command Line
```powershell
cd ObeservabiltyTraceabilty
dotnet run
```

Application output should show:
```
[12:34:56 INF] Starting application
```

---

## Step 3: Make Some API Requests

```powershell
# Get all orders (will generate logs)
$response = Invoke-WebRequest -Uri "https://localhost:7000/api/orders" `
	-SkipCertificateCheck

# Check response includes trace ID
$data = $response.Content | ConvertFrom-Json
Write-Host "Trace ID: $($data.traceId)"
```

---

## Step 4: Access Loki Logs in Grafana

### Navigate to Grafana
- URL: http://localhost:3000
- Username: `admin`
- Password: `admin`

### View Logs
1. Click **Explore** (left sidebar compass icon)
2. Select **Loki** from dropdown (top)
3. Click **Run Query** (or enter query below)

### First Query: See All Logs
```
{app="observability-app"}
```

### Search by Trace ID
```
{app="observability-app"} | json | TraceId="YOUR_TRACE_ID_HERE"
```

### Search by Level
```
{app="observability-app", level="error"}
```

### Search by Content
```
{app="observability-app"} |= "Retrieved"
```

---

## Common Queries

| Purpose | Query |
|---------|-------|
| All logs | `{app="observability-app"}` |
| Errors only | `{app="observability-app", level="error"}` |
| By trace ID | `{app="observability-app"} \| json \| TraceId="abc123"` |
| Contains text | `{app="observability-app"} \|= "error"` |
| Development logs | `{app="observability-app", environment="Development"}` |
| Last hour | Time range selector top-right |

---

## Troubleshooting

### Logs Not Appearing?

**Check 1: Loki is running**
```powershell
docker ps | findstr loki
```

**Check 2: Application is logging**
Look at console output - you should see log lines starting with `[HH:MM:SS INF]`

**Check 3: Loki is accessible**
```powershell
curl.exe http://localhost:3100/ready
# Should return "ready"
```

**Check 4: Check Loki logs**
```powershell
docker logs observability-loki
# Look for errors
```

**Solution: Restart Everything**
```powershell
# Stop containers
docker-compose down

# Remove volumes to start fresh
docker-compose down -v

# Start again
docker-compose up -d
```

---

## Understanding the Architecture

```
User Request
	 ↓
Your .NET App
	 ├─→ Console (immediate output)
	 └─→ Loki HTTP Sink (batched, async)
		   ↓
		 Loki (storage)
		   ↓
	   Grafana (visualization)
```

### Key Components

1. **Your App** - Logs via Serilog with two sinks:
   - Console (development)
   - HTTP to Loki (aggregation)

2. **Loki** - Receives logs, stores them, indexed by labels

3. **Grafana** - Queries Loki via LogQL language

---

## Available Endpoints

| Service | URL | Purpose |
|---------|-----|---------|
| Your API | https://localhost:7000 | Your application |
| Swagger | https://localhost:7000/swagger | API documentation |
| Prometheus | http://localhost:9090 | Metrics scraping |
| Loki | http://localhost:3100 | Log ingestion |
| Grafana | http://localhost:3000 | Dashboard & logs |
| PostgreSQL | localhost:5432 | Database |

---

## Useful Commands

### Docker Control
```powershell
# View all containers
docker-compose ps

# View logs
docker-compose logs -f loki        # Loki logs
docker-compose logs -f grafana     # Grafana logs

# Restart specific service
docker-compose restart loki

# Stop all
docker-compose down

# Stop and remove data
docker-compose down -v
```

### Test Loki API
```powershell
# Check Loki health
curl http://localhost:3100/ready

# Check Loki metrics
curl http://localhost:3100/metrics
```

### Test Application
```powershell
# Make request to API
curl https://localhost:7000/api/orders -SkipCertificateCheck

# Check Prometheus metrics
curl http://localhost:9090/metrics
```

---

## Next Steps

### 1. Create a Grafana Dashboard
1. Open Grafana (http://localhost:3000)
2. Click **Dashboards** > **+ New Dashboard**
3. Click **+ Add visualization**
4. Select **Loki** datasource
5. Enter query: `{app="observability-app"}`
6. Customize and save

### 2. Set Up Alerts (Optional)
1. In Grafana, go to **Alerting** > **Alert Rules**
2. Create rule for errors: `{level="error"}`
3. Set notification channel

### 3. Export Logs
1. In Grafana Explore
2. Run query
3. Click **Download CSV** or **Copy data**

### 4. Configure Retention
Edit `loki-config.yml` for storage policies

---

## Performance Tips

### For Development
- Logs batched every 5 seconds
- Perfect for debugging

### For Production (Future)
Edit `Program.cs`:
```csharp
Period = TimeSpan.FromSeconds(10)  // Wait longer
// Larger batches = better throughput
```

---

## Log Labels

Your logs are indexed with:

| Label | Example Value |
|-------|--------------|
| `app` | `observability-app` |
| `level` | `info`, `warning`, `error` |
| `environment` | `Development` |
| `request` | (controller/action) |

### Why Labels Matter
- Loki indexes labels (not full text)
- Query by labels is FAST
- Filter logs by label dimensions
- Find related logs quickly

---

## Sample Use Cases

### Use Case 1: Find Request Timeline
1. User reports: "My order stuck at 12:34"
2. Check response for trace ID: `abc-123-def`
3. Query: `{app="observability-app"} | json | TraceId="abc-123-def"`
4. See all logs for that request in order

### Use Case 2: Find All Errors
1. Query: `{app="observability-app", level="error"}`
2. See error patterns
3. Click log line for full exception
4. Correlation ID in response matches logs

### Use Case 3: Performance Analysis
1. Query: `{app="observability-app"} | json | duration > 5000`
2. Find slow requests
3. Check corresponding Prometheus metrics
4. Correlate both via trace ID

---

## Files Modified/Created

```
ObeservabiltyTraceabilty/
├── Program.cs ✓ (HTTP sink for Loki)
├── ObeservabiltyTraceabilty.csproj ✓ (Serilog.Sinks.Http)
├── docker-compose.yml ✓ (Added Loki service)
├── loki-config.yml ✓ (New - Loki config)
├── Logging/
│   ├── LokiHttpFormatProvider.cs ✓ (Formats logs for Loki)
│   ├── LokiBatchFormatter.cs ✓ (Helper for batch formatting)
│   └── LokiJsonFormatter.cs ✓ (JSON formatting)
├── grafana-provisioning/
│   ├── datasources/datasources.yml ✓ (Auto-config Loki)
│   └── dashboards/dashboards.yml ✓ (Auto-config dashboards)
└── LOKI_QUICKSTART.md (This file)
```

---

## Glossary

| Term | Meaning |
|------|---------|
| **LogQL** | Loki query language (like PromQL for Prometheus) |
| **Label** | Indexed dimension for filtering (fast) |
| **Stream** | Collection of logs with same labels |
| **Pipeline** | Log processing stage (parsing, filtering) |
| **Push** | Sending logs TO Loki (our HTTP sink) |
| **Scrape** | Pulling metrics FROM Prometheus |
| **TraceId** | Request correlation ID |
| **Trace ID** | Request correlation ID |

---

## Support & Debugging

### Logs not showing up?
1. Check Loki container is running: `docker ps | findstr loki`
2. Check application is logging: Look at console output
3. Check Loki accepts connections: `curl http://localhost:3100/ready`
4. Check `.WriteTo.Http` is in Program.cs
5. Restart containers: `docker-compose restart loki`

### Queries showing no results?
1. Check time range (top right of Grafana)
2. Verify you have some logs first: `{app="observability-app"}`
3. Check label names: `level` not `Level`
4. Wait a few seconds for logs to arrive

### Performance issues?
1. Reduce query time range
2. Filter by labels first (fast)
3. Avoid regex queries (slow)
4. Check Loki resources: `docker stats observability-loki`

---

## Quick Reference

```powershell
# Start stack
docker-compose up -d

# Run app
dotnet run

# Stop stack
docker-compose down

# View Grafana
start http://localhost:3000

# View Prometheus
start http://localhost:9090

# View Loki
start http://localhost:3100
```

---

## Summary

Your full observability stack is now running:

✅ **Metrics** - Prometheus scraping your app (`/metrics`)  
✅ **Logs** - Loki aggregating via HTTP sink  
✅ **Tracing** - Trace IDs correlating requests  
✅ **Visualization** - Grafana dashboards  

**All ready to use!** 🎉

Start with simple queries like `{app="observability-app"}` and build from there!
