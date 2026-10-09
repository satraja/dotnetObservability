# Observability & Traceability Application - Complete Guide

## 📊 Overview

This is a production-ready **observability and traceability application** built with .NET 10 featuring a complete monitoring stack:

### Three Pillars of Observability

```
┌─────────────────────────────────────────┐
│      OBSERVABILITY STACK               │
├─────────────────────────────────────────┤
│  📈 METRICS      (Prometheus)           │
│  📝 LOGS         (Loki & Grafana)       │
│  🔗 TRACES/IDs   (Distributed Tracing)  │
└─────────────────────────────────────────┘
```

---

## 🚀 Quick Start (5 minutes)

### Prerequisites
- Docker & Docker Compose
- .NET 10 SDK
- PowerShell or bash

### 1. Start Infrastructure
```powershell
docker-compose up -d
```

### 2. Run Application
```powershell
cd ObeservabiltyTraceabilty
dotnet run
```

### 3. Access Services
- **API**: https://localhost:7000
- **API Docs**: https://localhost:7000/swagger
- **Grafana**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090
- **Loki**: http://localhost:3100

---

## 📚 Documentation

### For Operators/DevOps
- **[LOKI_QUICKSTART.md](./LOKI_QUICKSTART.md)** - Get logs in Grafana in 5 minutes
- **[LOKI_IMPLEMENTATION.md](./LOKI_IMPLEMENTATION.md)** - Deep dive into Loki architecture

### For Developers
- **[TRACE_ID_GUIDE.md](./TRACE_ID_GUIDE.md)** - How to use trace IDs in code
- **[TRACE_ID_IMPLEMENTATION.md](./TRACE_ID_IMPLEMENTATION.md)** - Technical architecture
- **[PROMETHEUS_GUIDE.md](./PROMETHEUS_GUIDE.md)** - Metrics and instrumentation

### Setup & Configuration
- **[SETUP_GUIDE.md](./SETUP_GUIDE.md)** - Initial project setup
- **[README.md](./README.md)** - Project structure & models

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                                                           │
│  Your .NET 10 Application                               │
│  ├─ Controllers (OrdersController, ServicesController)  │
│  ├─ Services (business logic)                          │
│  ├─ Database (PostgreSQL)                              │
│  └─ Logging (Serilog)                                  │
│      ├─ Console (development)                          │
│      └─ HTTP → Loki (production)                       │
│                                                           │
└────────────────┬──────────────────────────────────────┘
				 │
	┌────────────┼────────────┐
	│            │            │
	▼            ▼            ▼
┌─────────┐ ┌────────┐ ┌──────────┐
│Prometheus│ │ Loki  │ │Trace IDs │
│(Metrics) │ │(Logs) │ │(Tracking)│
└────┬────┘ └───┬───┘ └────┬─────┘
	 │          │          │
	 └──────────┼──────────┘
				│
				▼
		  ┌──────────────┐
		  │   Grafana    │
		  │ (Dashboard)  │
		  └──────────────┘
```

---

## 📈 Metrics (Prometheus)

### What's Measured?
- API request duration
- API exception count
- API request volume
- Database query performance

### Access Metrics
1. **Prometheus UI**: http://localhost:9090
2. **Query examples**:
   ```
   rate(orders_api_duration_seconds[5m])        # Average API response time
   increase(api_exceptions_total[5m])           # Exceptions in last 5 minutes
   ```
3. **Your app endpoint**: https://localhost:7000/metrics

### Grafana Integration
- Prometheus datasource auto-configured
- Create custom dashboards
- Set up alerting rules

---

## 📝 Logs (Loki)

### What's Logged?
- Every HTTP request start/end
- Controller actions
- Database operations
- Errors and exceptions
- **Trace ID** with every log entry

### Access Logs
1. **Grafana Explore**: http://localhost:3000 → Explore → Loki
2. **Query examples**:
   ```
   {app="observability-app"}                           # All logs
   {app="observability-app", level="error"}           # Only errors
   {app="observability-app"} | json | TraceId="abc"   # By trace ID
   ```
3. **Loki API**: http://localhost:3100/loki/api/v1/query_range

---

## 🔗 Trace IDs (Distributed Tracing)

### What's a Trace ID?
Unique identifier assigned to each request, flows through:
- HTTP headers (X-Trace-Id)
- All logs
- API responses
- Database records

### Benefits
✅ Correlate logs across services  
✅ Track request from start to finish  
✅ Debug complex issues  
✅ Measure end-to-end latency  

### Usage
```csharp
// Get trace ID in controller
var traceId = HttpContext.GetTraceId();

// Returns in response
return Ok(new { data, traceId });

// In Grafana, query by trace ID
{app="observability-app"} | json | TraceId="your-id"
```

---

## 🗄️ Database

### PostgreSQL
- **Connection**: localhost:5432
- **Username**: postgres
- **Password**: postgres
- **Database**: ObservabilityTraceability

### Tables
```
Customers       (id, name, email)
Products        (id, name, description)
Services        (id, name, price)
Orders          (id, customerId, date)
Activities      (id, entityType, entityId, action, timestamp)
CustomerLogs    (id, customerId, logData, timestamp)
```

---

## 🛠️ API Endpoints

### Orders API
```
GET    /api/orders              - List all orders
GET    /api/orders/{id}         - Get order by ID
POST   /api/orders              - Create new order
PUT    /api/orders/{id}         - Update order
DELETE /api/orders/{id}         - Delete order
```

### Services API
```
GET    /api/services            - List all services
GET    /api/services/{id}       - Get service by ID
POST   /api/services            - Create new service
PUT    /api/services/{id}       - Update service
DELETE /api/services/{id}       - Delete service
```

### Other APIs
- `/api/customers` - Customer management
- `/api/products` - Product management
- `/api/activities` - Activity audit trail
- `/api/customerlogs` - Customer logs

---

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
	"PostgresConnection": "Host=localhost;Database=ObservabilityTraceability;...",
	"Loki": "http://localhost:3100"
  }
}
```

### Environment Variables (Optional)
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:LOKI_URI = "http://localhost:3100"
```

---

## 📦 Required Packages

- **Serilog.AspNetCore** - Structured logging
- **Serilog.Sinks.Console** - Console output
- **Serilog.Sinks.Http** - Loki integration
- **prometheus-net** - Prometheus metrics
- **prometheus-net.AspNetCore** - ASP.NET Core integration
- **Npgsql.EntityFrameworkCore.PostgreSQL** - Database
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI

---

## 🐳 Docker Compose Services

### Containers in Stack
| Service | Port | Purpose |
|---------|------|---------|
| PostgreSQL | 5432 | Database |
| Prometheus | 9090 | Metrics scraping |
| Loki | 3100 | Log ingestion |
| Grafana | 3000 | Dashboards |

### Common Commands
```powershell
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# View service status
docker-compose ps

# View logs
docker-compose logs -f loki

# Restart specific service
docker-compose restart grafana
```

---

## 🚦 Common Tasks

### Task 1: View API Logs for a Request
1. Make API request: `curl https://localhost:7000/api/orders -k`
2. Note the `traceId` in response
3. Go to Grafana → Explore → Loki
4. Query: `{app="observability-app"} | json | TraceId="YOUR_ID"`
5. See all logs for that request

### Task 2: Find Errors
1. Grafana → Explore → Loki
2. Query: `{app="observability-app", level="error"}`
3. Click logs to see full exception and stack trace

### Task 3: Performance Analysis
1. Prometheus → Graph tab
2. Query: `rate(orders_api_duration_seconds[5m])`
3. See average API response time over 5 minutes
4. Correlate with logs using trace ID

### Task 4: Database Connection Issues
1. Check PostgreSQL is running: `docker ps | findstr postgres`
2. Test connection:
   ```powershell
   psql -h localhost -U postgres -d ObservabilityTraceability
   ```
3. Check logs in Grafana: `{app="observability-app"} |= "database"`

---

## 🔍 Troubleshooting

### Application Won't Start
```powershell
# Check PostgreSQL is running
docker ps | findstr postgres

# Check database name
docker exec observability-postgres psql -U postgres -l

# Test connection string
# Edit appsettings.json PostgresConnection
```

### Logs Not in Loki
```powershell
# Check Loki container is running
docker ps | findstr loki

# Check Loki is healthy
curl http://localhost:3100/ready
# Should return "ready"

# Restart Loki
docker-compose restart loki
```

### Can't Access Grafana
```powershell
# Restart Grafana
docker-compose restart grafana

# Wait 30 seconds for startup
Start-Sleep -Seconds 30

# Access http://localhost:3000
```

### No MetricsEndpoint accessible
- Ensure `/metrics` endpoint is mapped in Program.cs
- Check Prometheus configuration points to correct URL
- Must be `/metrics` not `metrics`

---

## 📊 Monitoring Checklist

- [ ] Application running (check console output)
- [ ] PostgreSQL connected (can query data)
- [ ] Logs flowing to Loki (Grafana shows logs)
- [ ] Metrics being scraped (Prometheus /targets)
- [ ] Trace IDs in responses (check response body)
- [ ] Grafana dashboards working

---

## 🎯 Next Steps

### For Development
1. Update other controllers (follow OrdersController pattern)
2. Add custom metrics for your business logic
3. Create Grafana dashboards for your use cases

### For Production
1. Configure log retention policies
2. Set up Grafana alerts
3. Implement centralized authentication
4. Deploy to Kubernetes or cloud provider
5. Configure backup and disaster recovery

### Advanced Features
1. Implement distributed tracing (Jaeger/Zipkin)
2. Set up service-to-service trace propagation
3. Configure advanced alerting rules
4. Implement custom metric dashboards
5. Set up log-based SLOs

---

## 📖 Complete Documentation Index

| Document | Purpose | Audience |
|----------|---------|----------|
| [LOKI_QUICKSTART.md](./LOKI_QUICKSTART.md) | Get started in 5min | Everyone |
| [LOKI_IMPLEMENTATION.md](./LOKI_IMPLEMENTATION.md) | Loki architecture | DevOps/SRE |
| [PROMETHEUS_GUIDE.md](./PROMETHEUS_GUIDE.md) | Metrics guide | Developers |
| [TRACE_ID_GUIDE.md](./TRACE_ID_GUIDE.md) | Trace ID usage | Developers |
| [TRACE_ID_IMPLEMENTATION.md](./TRACE_ID_IMPLEMENTATION.md) | Trace ID tech | Architects |
| [TRACE_ID_INTEGRATION.md](./TRACE_ID_INTEGRATION.md) | Integration guide | Developers |
| [SETUP_GUIDE.md](./SETUP_GUIDE.md) | Initial setup | DevOps |
| [README.md](./README.md) | Project structure | Everyone |

---

## 🌟 Key Features

### ✅ Complete
- Multi-layer architecture (Controllers, Services, Repositories)
- PostgreSQL database with EF Core
- Structured logging to console and Loki
- Prometheus metrics (duration, counters, gauges)
- Automatic trace ID generation & propagation
- Serilog log enrichment
- Swagger/OpenAPI documentation
- Docker Compose for local dev

### 🔄 Integration Ready
- Loki configured for log aggregation
- Grafana configured for visualization
- Prometheus configured for metrics
- All three pillars of observability

### 🚀 Production Features
- Health checks
- Graceful shutdown
- Error handling
- Request validation
- Async operations
- Connection pooling

---

## 💡 Best Practices Implemented

✅ **Logging** - Structured logs with trace IDs  
✅ **Metrics** - Fine-grained prometheus instruments  
✅ **Error Handling** - Comprehensive exception handling  
✅ **Async/Await** - Non-blocking I/O operations  
✅ **DI Container** - Constructor injection throughout  
✅ **Repository Pattern** - Data access abstraction  
✅ **SOLID Principles** - Clean architecture  
✅ **Security** - HTTPS, input validation  

---

## 📞 Support

### Logs
1. Check console output first
2. Query Loki for detailed logs
3. Search by trace ID for request context

### Metrics
1. Check Prometheus targets
2. Query Prometheus for metrics
3. Create Grafana dashboards

### General Issues
1. Check Docker containers: `docker ps`
2. Check logs: `docker-compose logs -f SERVICE_NAME`
3. Verify connectivity to all services
4. Check appsettings.json configuration

---

## 🎓 Learning Resources

### Concepts
- [Observability Engineering](https://www.oreilly.com/library/view/observability-engineering/9781492076438/) - O'Reilly
- [Logs & Log Parsing](https://www.splunk.com/en_us/blog/learn/logs.html) - Splunk
- [Prometheus Best Practices](https://prometheus.io/docs/practices/instrumentation/) - Prometheus Docs

### Tools
- [Grafana Docs](https://grafana.com/docs/)
- [Loki Docs](https://grafana.com/docs/loki/latest/)
- [Prometheus Docs](https://prometheus.io/docs/)
- [Serilog Docs](https://serilog.net/)

---

## 📄 License

This project is provided as-is for educational and commercial use.

---

## ✨ Summary

This is a **complete, production-ready observability solution** with:

🎯 **Metrics** via Prometheus  
📊 **Logs** via Loki  
🔗 **Trace IDs** for correlation  
📈 **Grafana** for visualization  

All running locally via Docker Compose!

**Ready to monitor and understand your application!** 🚀
