# Developer Checklist - Observability & Traceability Application

## ✅ Pre-Deployment Checklist

### Code Quality
- [ ] Code builds successfully: `dotnet build`
- [ ] No compilation warnings
- [ ] Code follows existing conventions
- [ ] Proper error handling in place
- [ ] Logging is configured correctly

### Database
- [ ] PostgreSQL is running
- [ ] Connection string is correct in `appsettings.json`
- [ ] All migrations are applied: `dotnet ef database update`
- [ ] Database schema is verified
- [ ] Sample data loaded (optional)

### Configuration
- [ ] Application settings are correct
- [ ] Logging level is appropriate for environment
- [ ] Prometheus scrape interval configured
- [ ] API URLs are correct

### Testing
- [ ] API endpoints tested via Swagger UI
- [ ] Metrics endpoint is accessible: `/metrics`
- [ ] Prometheus can scrape metrics: `http://localhost:9090`
- [ ] All CRUD operations work
- [ ] Error handling is working

### Documentation
- [ ] README.md is complete
- [ ] GETTING_STARTED.md is accurate
- [ ] API endpoints are documented
- [ ] Deployment steps are clear

---

## 🚀 Getting Started (First Run)

### 1. Database Setup (Choose One)

**Option A: Docker Compose (Recommended)**
```powershell
docker-compose up -d
```

**Option B: Windows PowerShell**
```powershell
.\setup-db.ps1
```

**Option C: Linux/macOS Bash**
```bash
chmod +x setup-db.sh
./setup-db.sh
```

### 2. Create Database Schema
```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Run Application
```powershell
dotnet run
```

### 4. Access URLs
```
API: https://localhost:7000
Swagger: https://localhost:7000/swagger
Metrics: https://localhost:7000/metrics
Prometheus: http://localhost:9090
```

---

## 📋 Development Tasks

### Add a New Entity

1. **Create Model**
   ```csharp
   // Models/YourEntity.cs
   public class YourEntity
   {
	   public int YourEntityId { get; set; }
	   public string Name { get; set; }
	   // Add properties
   }
   ```

2. **Add DbSet to Context**
   ```csharp
   // Data/ApplicationDbContext.cs
   public DbSet<YourEntity> YourEntities { get; set; }
   ```

3. **Configure in OnModelCreating**
   ```csharp
   modelBuilder.Entity<YourEntity>()
	   .HasKey(x => x.YourEntityId);
   ```

4. **Create Migration**
   ```powershell
   dotnet ef migrations add Add_YourEntity
   dotnet ef database update
   ```

5. **Create Service**
   ```csharp
   // Services/YourEntityService.cs
   public interface IYourEntityService { }
   public class YourEntityService : IYourEntityService { }
   ```

6. **Create DTO**
   ```csharp
   // DTOs/DtoModels.cs
   public class YourEntityDto { }
   public class CreateYourEntityDto { }
   ```

7. **Create Controller**
   ```csharp
   // Controllers/YourEntitiesController.cs
   [ApiController]
   [Route("api/[controller]")]
   public class YourEntitiesController : ControllerBase { }
   ```

8. **Register in Program.cs**
   ```csharp
   builder.Services.AddScoped<IYourEntityService, YourEntityService>();
   ```

---

## 🔍 Common Development Tasks

### View Database Contents
```powershell
# Connect to PostgreSQL
psql -h localhost -U obstraceability -d ObservabilityTraceability

# List tables
\dt

# View table structure
\d "Orders"

# Query data
SELECT * FROM "Orders";
```

### Check Metrics
```powershell
# In browser or curl
curl -k https://localhost:7000/metrics
```

### View Logs in Real-time
```powershell
dotnet run 2>&1 | tee app.log
```

### Reset Database
```powershell
# Remove last migration
dotnet ef migrations remove

# Remove migrations folder
Remove-Item Migrations -Recurse -Force

# Recreate migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Test API Endpoint
```powershell
# PowerShell - GET request
$response = Invoke-WebRequest -Uri "https://localhost:7000/api/orders" -SkipCertificateCheck
$response.Content | ConvertFrom-Json

# PowerShell - POST request
$body = @{
	orderNumber = "ORD-001"
	customerId = 1
	productId = 1
	serviceId = 1
} | ConvertTo-Json

$response = Invoke-WebRequest `
	-Uri "https://localhost:7000/api/orders" `
	-Method Post `
	-Headers @{"Content-Type"="application/json"} `
	-Body $body `
	-SkipCertificateCheck

# Using curl
curl -k -X GET "https://localhost:7000/api/orders"
```

---

## 🐛 Debugging

### Enable Debug Logging
```csharp
// In Program.cs
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()  // Change to Debug
	.WriteTo.Console()
	.CreateLogger();
```

### Database Query Logging
```csharp
// In Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options
		.UseNpgsql(connectionString)
		.LogTo(Console.WriteLine)  // Add this line
);
```

### Enable Swagger in Production (Not Recommended)
```csharp
// In Program.cs
if (app.Environment.IsDevelopment() || true)  // Remove || true in production
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
```

---

## 📊 Monitoring Tasks

### Set Up Prometheus Queries

**View request rate:**
```
rate(order_api_duration_seconds_count[5m])
```

**View error rate:**
```
rate(api_exceptions_total[5m])
```

**View average response time:**
```
rate(order_api_duration_seconds_sum[5m]) / rate(order_api_duration_seconds_count[5m])
```

### Create Grafana Dashboard
1. Visit http://localhost:3000
2. Add Prometheus data source
3. Create new dashboard
4. Add panels with queries above

---

## 🔒 Security Checklist

- [ ] Connection strings are NOT in source code (use appsettings + secrets)
- [ ] Sensitive data is not logged
- [ ] SQL injection is prevented (using EF Core)
- [ ] HTTPS is enforced in production
- [ ] API keys/tokens are secured (if added)
- [ ] Database backups are automated
- [ ] Error messages don't expose internals

---

## 🚢 Deployment Checklist

### Before Deploying to Production

- [ ] Run `dotnet publish -c Release` for optimized build
- [ ] Test with production connection string
- [ ] Verify metrics are being collected
- [ ] Check logs for any warnings
- [ ] Ensure database backups are configured
- [ ] Set appropriate logging levels
- [ ] Verify Prometheus retention policy
- [ ] Test error handling scenarios
- [ ] Load test the application

### Docker Deployment
```powershell
# Build image
docker build -t observability-app:1.0 .

# Run container
docker run -p 7000:7000 `
	-e ConnectionStrings__PostgresConnection="..." `
	observability-app:1.0
```

---

## 📝 Useful SQL Queries

### View Recent Activities
```sql
SELECT * FROM "Activities"
ORDER BY "Timestamp" DESC
LIMIT 10;
```

### Count Objects by Type
```sql
SELECT "EntityType", COUNT(*) as Count
FROM "Activities"
GROUP BY "EntityType";
```

### Find All Changes to an Order
```sql
SELECT * FROM "Activities"
WHERE "EntityType" = 'Order' AND "EntityId" = 1
ORDER BY "Timestamp" DESC;
```

### View Customer Logs
```sql
SELECT l.*, c."Name", o."OrderNumber"
FROM "CustomerLogs" l
LEFT JOIN "Customers" c ON l."CustomerId" = c."CustomerId"
LEFT JOIN "Orders" o ON l."OrderId" = o."OrderId"
ORDER BY l."Timestamp" DESC;
```

---

## 🎯 Performance Optimization Tips

1. **Database Indexes**
   - Add indexes on frequently queried fields
   - `ALTER TABLE "Orders" ADD INDEX idx_customer_id ("CustomerId");`

2. **Query Optimization**
   - Use `Include()` for related entities to avoid N+1 queries
   - Consider pagination for large result sets

3. **Caching**
   - Add Redis for caching frequently accessed data
   - Cache Prometheus metrics

4. **Connection Pooling**
   - Already configured in connection string
   - Monitor connection pool settings

5. **Metrics**
   - Monitor `database_query_duration_seconds` histogram
   - Alert if response times spike

---

## 🆘 Troubleshooting Guide

| Issue | Solution |
|-------|----------|
| Port 7000 in use | Change port in `launchSettings.json` |
| Database connection failed | Verify PostgreSQL is running, check connection string |
| Migrations won't apply | Delete `Migrations` folder, recreate migration |
| Metrics not showing | Ensure Prometheus is running and configured |
| Swagger not loading | Clear browser cache, ensure HTTPS is available |
| Docker build fails | Check Docker Desktop is running, verify Dockerfile |

---

## 📞 Quick Reference

### Build Commands
```powershell
dotnet build              # Build
dotnet clean              # Clean
dotnet restore            # Restore packages
dotnet publish -c Release # Publish for production
```

### Database Commands
```powershell
dotnet ef migrations add <Name>    # Create migration
dotnet ef database update          # Apply migrations
dotnet ef database drop            # Drop database
dotnet ef migrations remove        # Remove last migration
```

### Run Commands
```powershell
dotnet run                         # Run with debug info
dotnet run --no-build             # Run without rebuilding
dotnet run --urls="https://localhost:7001"  # Custom port
```

### Docker Commands
```powershell
docker-compose up -d               # Start services
docker-compose down                # Stop services
docker-compose logs -f postgres    # View logs
docker exec -it <container> bash   # Access container
```

---

## ✅ Final Verification

Before declaring the project ready:

- [ ] Application starts without errors
- [ ] All API endpoints return expected responses
- [ ] Database operations work correctly
- [ ] Prometheus metrics are being collected
- [ ] Swagger UI is accessible
- [ ] Activity logging captures changes
- [ ] Customer logs can be created
- [ ] No console errors or warnings
- [ ] Performance is acceptable
- [ ] Documentation is complete

---

**Once all checks are complete, your application is ready for use!** 🎉
