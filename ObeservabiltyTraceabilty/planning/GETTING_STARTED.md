# Getting Started Guide

## Quick Start (5 minutes)

### Option 1: Using Docker Compose (Recommended)

This is the easiest way to get everything running quickly.

#### Prerequisites
- Docker Desktop (includes Docker and Docker Compose)
- .NET 10 SDK

#### Steps

1. **Start PostgreSQL and Prometheus with Docker Compose**
   ```powershell
   docker-compose up -d
   ```
   This will start:
   - PostgreSQL on `localhost:5432`
   - Prometheus on `localhost:9090`
   - Grafana on `localhost:3000` (optional, for visualization)

2. **Build and Run the Application**
   ```powershell
   cd ObeservabiltyTraceabilty
   dotnet restore
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   dotnet run
   ```

3. **Access the Application**
   - API: https://localhost:7000
   - Swagger UI: https://localhost:7000/swagger
   - Prometheus: http://localhost:9090
   - Grafana: http://localhost:3000 (username: admin, password: admin)

4. **Stop Services**
   ```powershell
   docker-compose down
   ```

---

### Option 2: Manual Setup (Windows)

#### Prerequisites
- PostgreSQL (version 12+) installed
- .NET 10 SDK
- Prometheus (optional)

#### Step 1: Create Database

Run the PowerShell setup script (as Administrator):

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser -Force
.\setup-db.ps1
```

Or manually using pgAdmin / psql:

```sql
CREATE DATABASE "ObservabilityTraceability";
CREATE USER obstraceability WITH PASSWORD 'obstraceability123';
GRANT ALL PRIVILEGES ON DATABASE "ObservabilityTraceability" TO obstraceability;
ALTER DATABASE "ObservabilityTraceability" OWNER TO obstraceability;
```

#### Step 2: Update Connection String

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
	"PostgresConnection": "Host=localhost;Database=ObservabilityTraceability;Username=obstraceability;Password=obstraceability123;Port=5432"
  }
}
```

#### Step 3: Create Database Schema

```powershell
cd ObeservabiltyTraceabilty
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### Step 4: Run Application

```powershell
dotnet run
```

The application will start at: `https://localhost:7000`

---

### Option 3: Manual Setup (Linux/macOS)

#### Prerequisites
- PostgreSQL installed
- .NET 10 SDK
- Bash/Zsh shell

#### Step 1: Create Database

Make the setup script executable:

```bash
chmod +x setup-db.sh
./setup-db.sh
```

#### Step 2-4: Follow steps 2-4 from Windows setup (commands are the same)

---

## First Time Usage

### 1. Access the Application

**Swagger/OpenAPI Documentation:**
```
https://localhost:7000/swagger
```

### 2. Create Sample Data

Use the Swagger UI to create test data:

**Create a Customer:**
```bash
POST /api/customers
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com"
}
```

**Create a Product:**
```bash
POST /api/products
Content-Type: application/json

{
  "productName": "Laptop Pro",
  "description": "High-performance laptop",
  "price": 1299.99
}
```

**Create a Service:**
```bash
POST /api/services
Content-Type: application/json

{
  "serviceName": "Premium Support",
  "description": "24/7 technical support",
  "serviceType": "Support",
  "price": 99.99
}
```

**Create an Order:**
```bash
POST /api/orders
Content-Type: application/json

{
  "orderNumber": "ORD-001",
  "customerId": 1,
  "productId": 1,
  "serviceId": 1
}
```

### 3. View Metrics

Visit Prometheus UI:
```
http://localhost:9090
```

Search for metrics like:
- `orders_created_total`
- `products_created_total`
- `services_created_total`
- `order_api_duration_seconds`

### 4. Create Customer Logs

```bash
POST /api/customerlogs
Content-Type: application/json

{
  "customerId": 1,
  "orderId": 1,
  "logMessage": "Customer placed order",
  "logLevel": "Info",
  "icon": "success"
}
```

### 5. View Activity Audit Trail

Get activities for an order:
```bash
GET /api/activities/entity/Order/1
```

---

## Troubleshooting

### My database won't connect

**Check PostgreSQL is running:**
```powershell
# Windows
Get-Service postgresql*

# Linux/macOS
sudo systemctl status postgresql
```

**Verify connection string** in `appsettings.json` matches your setup.

### Port 5432 is already in use

Change the port in connection string or stop the service using that port:

```powershell
# Find process using port 5432
Get-NetTCPConnection -LocalPort 5432

# Or use Docker to run on a different port
docker run -p 5433:5432 postgres:16-alpine
```

Update connection string to use `Port=5433`

### Migrations fail

**Clean and reset:**
```powershell
# Remove last migration
dotnet ef migrations remove

# Remove all migrations
Remove-Item Migrations -Recurse -Force

# Recreate
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Application won't start

Check the logs:
```powershell
dotnet run 2>&1 | tee app.log
```

Common issues:
- Database connection string is wrong
- Database doesn't exist
- .NET 10 SDK not installed (`dotnet --version`)
- Port 7000 already in use

### Docker container won't start

Check logs:
```powershell
docker-compose logs postgres
docker-compose logs prometheus
```

Full reset:
```powershell
docker-compose down -v
docker-compose up -d
```

---

## Project Structure

```
ObeservabiltyTraceabilty/
├── Models/                     # Entity models
├── Data/                      # Database context
├── Repositories/              # Data access patterns
├── Services/                  # Business logic
├── Controllers/               # API endpoints
├── DTOs/                      # Data transfer objects
├── Migrations/                # EF Core migrations
├── appsettings.json          # Configuration
├── Program.cs                # Application startup
├── prometheus.yml            # Prometheus config
├── docker-compose.yml        # Docker services
├── setup-db.ps1             # Database setup (Windows)
├── setup-db.sh              # Database setup (Linux/macOS)
├── README.md                # Full documentation
└── GETTING_STARTED.md       # This file
```

---

## Next Steps

1. **Explore APIs** - Use Swagger UI to test all endpoints
2. **Monitor Metrics** - Watch Prometheus collect metrics in real-time
3. **Setup Grafana** - Create visualizations of your metrics
4. **Customize** - Modify models and services for your needs
5. **Deploy** - Use Docker or deploy to cloud (Azure, AWS, etc.)

---

## Support & Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

---

## Quick Commands Reference

```powershell
# Build project
dotnet build

# Run application
dotnet run

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# View EF Core logs
dotnet ef database update --verbose

# Docker operations
docker-compose up -d          # Start services
docker-compose down           # Stop services
docker-compose logs -f        # View logs
docker-compose ps             # List containers

# Clean and rebuild
dotnet clean
dotnet build

# Test API
curl -X GET "https://localhost:7000/api/orders" -k

# Swagger UI
https://localhost:7000/swagger

# Metrics endpoint
https://localhost:7000/metrics

# Prometheus UI
http://localhost:9090
```

---

## Common Configuration Changes

### Change Application Port

Edit `appsettings.json` or use command line:
```powershell
dotnet run --urls="https://localhost:7001"
```

### Change Database

Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
	"PostgresConnection": "Host=your-host;Database=your-db;Username=your-user;Password=your-password"
  }
}
```

### Adjust Logging Level

In `Program.cs`:
```csharp
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()  // Change to Debug for more logs
	.WriteTo.Console()
	.CreateLogger();
```

---

Happy coding! 🚀
