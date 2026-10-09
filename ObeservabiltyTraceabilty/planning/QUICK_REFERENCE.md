# Quick Reference Card

## 🚀 Start Application (Choose One)

### Docker (Easiest)
```powershell
docker-compose up -d
dotnet run
```

### Manual Setup
```powershell
.\setup-db.ps1
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

---

## 🌐 Access Points
| Service | URL |
|---------|-----|
| Application | https://localhost:7000 |
| API Docs | https://localhost:7000/swagger |
| Metrics | https://localhost:7000/metrics |
| Prometheus | http://localhost:9090 |

---

## 📡 API Endpoints Quick List

### Orders
```
GET    /api/orders              # List all
GET    /api/orders/1            # Get by ID
POST   /api/orders              # Create
PUT    /api/orders/1            # Update
DELETE /api/orders/1            # Delete
```

### Products
```
GET    /api/products
GET    /api/products/1
POST   /api/products
PUT    /api/products/1
DELETE /api/products/1
```

### Services
```
GET    /api/services
GET    /api/services/1
POST   /api/services
PUT    /api/services/1
DELETE /api/services/1
```

### Activities (Audit Log)
```
GET    /api/activities
GET    /api/activities/1
GET    /api/activities/entity/Order/1
```

### Customer Logs
```
GET    /api/customerlogs
GET    /api/customerlogs/1
GET    /api/customerlogs/customer/1
GET    /api/customerlogs/order/1
POST   /api/customerlogs
```

---

## 📊 Example API Calls

### Create Product
```bash
curl -k -X POST https://localhost:7000/api/products \
  -H "Content-Type: application/json" \
  -d '{
	"productName": "Laptop",
	"description": "High-performance laptop",
	"price": 1299.99
  }'
```

### Create Service
```bash
curl -k -X POST https://localhost:7000/api/services \
  -H "Content-Type: application/json" \
  -d '{
	"serviceName": "Support",
	"description": "24/7 support",
	"serviceType": "Support",
	"price": 99.99
  }'
```

### Create Order
```bash
curl -k -X POST https://localhost:7000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
	"orderNumber": "ORD-001",
	"customerId": 1,
	"productId": 1,
	"serviceId": 1
  }'
```

### Get Metrics
```bash
curl -k https://localhost:7000/metrics
```

---

## 🔧 Build & Database Commands

```powershell
# Build
dotnet build
dotnet clean
dotnet restore

# Database
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef database drop

# Run
dotnet run
dotnet run --urls="https://localhost:7001"

# Docker
docker-compose up -d
docker-compose down
docker-compose logs -f
```

---

## 📈 Prometheus Queries

```
# Orders created per minute
rate(orders_created_total[1m])

# API response time
rate(order_api_duration_seconds_sum[5m]) / rate(order_api_duration_seconds_count[5m])

# Total exceptions
rate(api_exceptions_total[5m])

# Active orders
active_orders_count

# Database query time
histogram_quantile(0.95, rate(database_query_duration_seconds_bucket[5m]))
```

---

## 📝 Common Tasks

### Reset Database
```powershell
dotnet ef database drop
dotnet ef database update
```

### View Recent Activities
```powershell
curl -k https://localhost:7000/api/activities | ConvertFrom-Json | Sort -Property Timestamp -Descending | Select -First 10
```

### Check Metrics
```powershell
curl -k https://localhost:7000/metrics | Select-String "order"
```

### Database Query
```powershell
psql -h localhost -U obstraceability -d ObservabilityTraceability
SELECT * FROM "Orders";
```

---

## ⚠️ Troubleshooting Quick Fix

| Problem | Fix |
|---------|-----|
| Port 7000 busy | `dotnet run --urls="https://localhost:7001"` |
| Database connection error | Check `appsettings.json` connection string |
| Swagger not loading | Clear browser cache, hard refresh (Ctrl+Shift+R) |
| Metrics missing | Verify Prometheus config, check `/metrics` endpoint |
| Docker won't start | `docker-compose down -v && docker-compose up -d` |

---

## 🎯 Next Steps

1. ✅ Start application
2. ✅ Test APIs in Swagger UI
3. ✅ Create sample data
4. ✅ View metrics in Prometheus
5. ✅ Monitor activities in API
6. ✅ Setup Grafana dashboards (optional)

---

## 📚 Documentation

- **README.md** - Full documentation
- **GETTING_STARTED.md** - Setup guide
- **IMPLEMENTATION_SUMMARY.md** - What was built
- **DEVELOPER_CHECKLIST.md** - Development guide

---

**Status**: ✅ Ready to Run
**Framework**: .NET 10
**Database**: PostgreSQL
**Monitoring**: Prometheus + Grafana
