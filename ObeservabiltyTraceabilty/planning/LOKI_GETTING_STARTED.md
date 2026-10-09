# Getting Started with Loki - Visual Guide

## 🎯 60-Second Start

### 1. Open PowerShell
```powershell
cd D:\workplace\dotnet\ObeservabiltyTraceabilty\
```

### 2. Start Docker Stack
```powershell
docker-compose up -d
```

Wait for output:
```
[+] Running 5/5 ✓
 ✓ observability-postgres
 ✓ observability-prometheus  
 ✓ observability-loki
 ✓ observability-grafana
```

### 3. Run Application
```powershell
cd ObeservabiltyTraceabilty
dotnet run
```

### 4. Make a Request (New PowerShell)
```powershell
$r = curl https://localhost:7000/api/orders -k
$r.Content | ConvertFrom-Json
```

### 5. Open Grafana
```powershell
start http://localhost:3000
```

### 6. View Your Logs
1. Login: `admin` / `admin`
2. Click: **Explore** (left sidebar)
3. Select: **Loki** (dropdown top)
4. Enter: `{app="observability-app"}`
5. Click: **Run Query**

✅ **You're seeing your logs from Loki!**

---

## 🗺️ Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                  YOUR CODE                              │
│         (OrdersController, etc.)                        │
├─────────────────────────────────────────────────────────┤
│              SERILOG LOGGING                            │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Enrich with:                                    │   │
│  │ • Trace ID                                      │   │
│  │ • Environment                                   │   │
│  │ • Request info                                  │   │
│  └─────────────────────────────────────────────────┘   │
├──────────────────┬──────────────────────────────────────┤
│                  │                                      │
│          ┌───────▼─────────┐                            │
│          │ LocalConsole    │  (Development)             │
│          │ :standard out   │                            │
│          └─────────────────┘                            │
│                  │                                      │
│          ┌───────▼────────────────────────────────────┐ │
│          │ HTTP Sink → Loki                           │ │
│          │ POST /loki/api/v1/push                     │ │
│          │ Batches: every 5 seconds or 10 events     │ │
│          └────────────┬────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
					 │ HTTP
		┌────────────▼──────────────┐
		│   LOKI (Port 3100)         │
		│   ├─ Ingester             │
		│   ├─ Distributor          │
		│   └─ Storage              │
		└────────────┬──────────────┘
					 │
		┌────────────▼──────────────┐
		│  GRAFANA (Port 3000)       │
		│  ├─ Explore Tab            │
		│  ├─ LogQL Queries          │
		│  └─ Dashboards             │
		└───────────────────────────┘
```

---

## 📍 Where Everything Runs

| What | Where | URL |
|------|-------|-----|
| Your API | localhost | https://localhost:7000 |
| API Docs | localhost | https://localhost:7000/swagger |
| Prometheus Metrics | Docker | http://localhost:9090 |
| **Loki Logs** | Docker | http://localhost:3100 |
| **Grafana UI** | Docker | http://localhost:3000 |
| PostgreSQL | Docker | localhost:5432 |

---

## 🔍 Finding Your Logs

### Example 1: "I made a request, where are the logs?"

```powershell
# Step 1: Make request and get trace ID
$r = curl https://localhost:7000/api/orders -k
$data = $r.Content | ConvertFrom-Json
$traceId = $data.traceId
Write-Host "Trace ID: $traceId"
# Output: Trace ID: 20240115120530-a1b2c3d4e5f1g2h3

# Step 2: Go to Grafana → Explore → Loki
# Step 3: Enter query:
# {app="observability-app"} | json | TraceId="20240115120530-a1b2c3d4e5f1g2h3"

# Step 4: Click "Run Query"
# Result: All logs for that single request!
```

### Example 2: "Show me all errors"

Go to Grafana → Explore → Loki:
```
{app="observability-app", level="error"}
```

### Example 3: "Search for a specific message"

Go to Grafana → Explore → Loki:
```
{app="observability-app"} |= "Retrieved"
```

---

## 📊 What You'll See

### Console Output (Your Terminal)
```
[12:05:30 INF] Starting application
[12:05:31 INF] TraceId: 20240115120531-a1b2c3d4e5 GET /api/orders started
[12:05:31 INF] TraceId: 20240115120531-a1b2c3d4e5 Retrieved 5 orders
```

### Grafana Explore (Browser)
```
{app="observability-app"}

Results:
[12:05:31] TraceId: 20240115120531... GET /api/orders started
[12:05:31] TraceId: 20240115120531... Retrieved 5 orders
[12:05:31] TraceId: 20240115120531... Request completed
```

### Response JSON (API Response)
```json
{
  "success": true,
  "data": [...],
  "traceId": "20240115120531-a1b2c3d4e5",
  "count": 5
}
```

---

## 🎛️ Control Panel

### Docker Compose Commands
```powershell
# See all running containers
docker ps

# Stop everything
docker-compose down

# Start everything
docker-compose up -d

# View logs for a service
docker logs observability-loki
docker logs observability-grafana

# Restart a service
docker-compose restart loki
docker-compose restart grafana
```

### Application Commands
```powershell
# Run with debug output
dotnet run

# Run as background service
dotnet run &

# Build
dotnet build

# Test
dotnet test
```

---

## 🛠️ Troubleshooting Flowchart

```
"Loki logs not showing in Grafana?"
│
├─→ "Are containers running?"
│   ├─→ No → Run: docker-compose up -d
│   └─→ Yes → Next
│
├─→ "Is Loki healthy?"
│   ├─→ curl http://localhost:3100/ready
│   ├─→ No → Run: docker-compose restart loki
│   └─→ Yes → Next
│
├─→ "Are logs being generated?"
│   ├─→ Check console output (should show logs)
│   ├─→ No → Check application is running
│   └─→ Yes → Next
│
├─→ "Did you query correctly?"
│   ├─→ Try: {app="observability-app"}
│   ├─→ Still nothing → Check labels
│   └─→ Working → Done!
```

---

## 📈 Performance Indicators

### Healthy System
```
✅ Console logs appear immediately
✅ Grafana shows logs within 5 seconds
✅ Queries return in <500ms
✅ No errors in container logs
✅ CPU/Memory stable
```

### Warning Signs
```
⚠️ Logs delayed >10 seconds
⚠️ Grafana queries slow (>2s)
⚠️ High CPU usage (>80%)
⚠️ High memory usage (>2GB)
⚠️ Errors in Loki logs
```

### Actions to Take
```
1. Check disk space: docker stats
2. Restart services: docker-compose restart
3. Review logs: docker logs observability-loki
4. Scale down: reduce log volume
5. Check queries: simplify your query
```

---

## 🚀 Sample Workflows

### Workflow 1: Debug a Request
```
1. User reports: "Order 123 failed"
2. Get trace ID from error response
3. Grafana → Explore → Loki
4. Query: {app...} | json | TraceId="xxx"
5. See exact sequence of operations
6. Find root cause in logs
7. Fix bug
```

### Workflow 2: Monitor Performance
```
1. Grafana → Explore → Prometheus
2. Query: rate(orders_api_duration_seconds[5m])
3. See graph of API response times
4. If spike detected:
   - Switch to Loki
   - Query: {app...} from same time range
   - Find what caused the slowdown
5. Optimize/Fix
```

### Workflow 3: Find All Errors
```
1. Grafana → Explore → Loki
2. Query: {app="observability-app", level="error"}
3. See all error events
4. Click error to see full stack trace
5. Find patterns
6. Add monitoring/alerting for that pattern
```

---

## 📚 Recommended Next Steps

### Today (Next 30 minutes)
- [ ] Start docker-compose
- [ ] Run application
- [ ] Make API requests
- [ ] View logs in Grafana
- [ ] Try different queries

### This Week
- [ ] Read LOKI_QUICKSTART.md
- [ ] Explore LogQL query language
- [ ] Create first custom dashboard
- [ ] Test error scenarios
- [ ] Share with team

### This Month
- [ ] Production deployment plan
- [ ] Set up Grafana alerts
- [ ] Document key queries/dashboards
- [ ] Train team on usage
- [ ] Establish log retention policy

---

## 🎓 Query Language Cheat Sheet

```
# Basic Queries
{app="observability-app"}                          # All logs
{app="observability-app", level="error"}          # Only errors
{app="observability-app", environment="Dev"}      # By environment

# Text Filtering
{app="observability-app"} |= "Retrieved"          # Contains text
{app="observability-app"} != "DEBUG"              # Doesn't contain
{app="observability-app"} |~ "error|Error"        # Regex match

# JSON Parsing
{app="observability-app"} | json                  # Parse JSON
{app="observability-app"} | json | TraceId="x"   # Filter parsed JSON
{app="observability-app"} | json | level="error"  # Another JSON field

# Time-based (use Grafana time picker on right)
Last 1 hour                                       # Quick selector
Last 24 hours
Custom range
```

---

## 🎯 Key Takeaways

1. **Logs are centralized** in Loki (not scattered in files)
2. **Every request has a Trace ID** for correlation
3. **Grafana provides visibility** with LogQL queries
4. **Label-based indexing** makes queries fast
5. **Docker Compose** makes local dev easy
6. **HTTP integration** keeps it simple

---

## ✨ You're All Set!

```
┌───────────────────────────────────────┐
│   ✅ LOKI IMPLEMENTATION COMPLETE    │
├───────────────────────────────────────┤
│ • Logs → Loki → Grafana               │
│ • Trace IDs automatic                 │
│ • Queries ready to use                │
│ • Production-ready                    │
│ • Documentation complete              │
└───────────────────────────────────────┘
```

### Quick Start Command
```powershell
docker-compose up -d; Start-Sleep -s 5; dotnet run
```

### Access Everything
```powershell
start http://localhost:3000  # Grafana
start http://localhost:9090  # Prometheus  
start http://localhost:7000  # Your API
```

---

## 📞 When You Need Help

| Issue | Solution |
|-------|----------|
| Containers won't start | `docker-compose down -v && docker-compose up -d` |
| No logs in Grafana | Check console output, verify Loki health |
| Slow queries | Filter by labels first, reduce time range |
| Out of memory | Lower log level, reduce ingestion rate |
| Can't connect to DB | Check PostgreSQL container, verify connection string |

---

## 🎉 Welcome to Observability!

You now have:
- 📈 Metrics (Prometheus)
- 📝 Logs (Loki)
- 🔗 Trace IDs
- 📊 Visualization (Grafana)

**Your application is now fully observable!** 🚀

Questions? See the documentation files or check the troubleshooting guide above!
