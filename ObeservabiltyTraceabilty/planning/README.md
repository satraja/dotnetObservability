# Observability & Traceability Application

A comprehensive .NET 10 ASP.NET Core application for managing Orders, Products, Services with full activity tracking and Prometheus metrics integration.

## Architecture Overview

### Layers
- **Presentation Layer**: ASP.NET Core Controllers (RESTful APIs)
- **Business Logic Layer**: Services with business rules
- **Data Access Layer**: Repository Pattern with EF Core
- **Data Storage**: PostgreSQL Database

### Key Features
✅ Order Management (CRUD Operations)
✅ Product Management (CRUD Operations)
✅ Service Management (CRUD Operations)
✅ Activity Logging & Traceability (Full audit trail)
✅ Customer Logs with Icons & Status
✅ Prometheus Metrics Integration
✅ Structured Logging with Serilog
✅ OpenAPI/Swagger Documentation

## Technologies Used

- **.NET 10** - ASP.NET Core
- **PostgreSQL** - Relational Database
- **Entity Framework Core 8** - ORM
- **Prometheus** - Metrics Collection
- **Serilog** - Structured Logging
- **Repository Pattern** - Data Access Abstraction
- **Dependency Injection** - Built-in ASP.NET Core DI

## Project Structure

```
ObeservabiltyTraceabilty/
├── Models/                    # Entity Models
│   ├── Customer.cs
│   ├── Product.cs
│   ├── Service.cs
│   ├── Order.cs
│   ├── Activity.cs
│   └── CustomerLog.cs
├── Data/
│   └── ApplicationDbContext.cs # Entity Framework DbContext
├── Repositories/              # Repository Pattern
│   ├── IRepository.cs
│   └── Repository.cs
├── Services/                  # Business Logic
│   ├── OrderService.cs
│   ├── ProductService.cs
│   ├── ServiceService.cs
│   ├── ActivityService.cs
│   ├── CustomerLogService.cs
│   └── MetricsService.cs      # Prometheus Metrics
├── Controllers/               # API Endpoints
│   ├── OrdersController.cs
│   ├── ProductsController.cs
│   ├── ServicesController.cs
│   ├── ActivitiesController.cs
│   └── CustomerLogsController.cs
├── DTOs/
│   └── DtoModels.cs          # Data Transfer Objects
├── Program.cs                # Application Entry Point
├── appsettings.json          # Configuration
└── ObeservabiltyTraceabilty.csproj
```

## Setup Instructions

### Prerequisites
- .NET 10 SDK
- PostgreSQL (version 12 or higher)
- Visual Studio 2026 Community or VS Code

### 1. Database Setup

```powershell
# Using PowerShell or Command Prompt

# Create database
psql -U postgres -c "CREATE DATABASE \"ObservabilityTraceability\";"

# Or manually create using pgAdmin or DBeaver
```

### 2. Configuration

Update `appsettings.json` with your PostgreSQL connection:

```json
{
  "ConnectionStrings": {
	"PostgresConnection": "Host=localhost;Database=ObservabilityTraceability;Username=postgres;Password=YOUR_PASSWORD;Port=5432"
  }
}
```

### 3. Install Dependencies

```powershell
# Navigate to project directory
cd ObeservabiltyTraceabilty

# Restore packages
dotnet restore
```

### 4. Create Database Schema

```powershell
# Install Entity Framework CLI (if not installed)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update
```

### 5. Run Application

```powershell
dotnet run

# Or using Visual Studio: Press F5
```

The application will start at: `https://localhost:7000` (or `https://localhost:5000`)

## API Endpoints

### Base URL
```
https://localhost:7000/api
```

### Orders
- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create new order
- `PUT /api/orders/{id}` - Update order
- `DELETE /api/orders/{id}` - Delete order

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Services
- `GET /api/services` - Get all services
- `GET /api/services/{id}` - Get service by ID
- `POST /api/services` - Create new service
- `PUT /api/services/{id}` - Update service
- `DELETE /api/services/{id}` - Delete service

### Activities (Audit Trail)
- `GET /api/activities` - Get all activities
- `GET /api/activities/{id}` - Get activity by ID
- `GET /api/activities/entity/{entityType}/{entityId}` - Get activities for specific entity

### Customer Logs
- `GET /api/customerlogs` - Get all customer logs
- `GET /api/customerlogs/{id}` - Get log by ID
- `GET /api/customerlogs/customer/{customerId}` - Get logs by customer
- `GET /api/customerlogs/order/{orderId}` - Get logs by order
- `POST /api/customerlogs` - Create new log

## Prometheus Metrics

### Access Metrics
Metrics are available at: `https://localhost:7000/metrics`

### Key Metrics Tracked

**Counters:**
- `orders_created_total` - Total orders created
- `orders_updated_total` - Total orders updated
- `orders_deleted_total` - Total orders deleted
- `products_created_total` - Total products created
- `services_created_total` - Total services created
- `activities_logged_total` - Total activities logged
- `api_exceptions_total` - Total API exceptions

**Gauges:**
- `active_orders_count` - Current active orders
- `active_services_count` - Current active services
- `database_connection_pool_size` - DB connection pool status

**Histograms:**
- `order_api_duration_seconds` - Order API response time
- `product_api_duration_seconds` - Product API response time
- `service_api_duration_seconds` - Service API response time
- `database_query_duration_seconds` - Database query latency

## Prometheus Configuration

Create `prometheus.yml` in your Prometheus directory:

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'observability-app'
	static_configs:
	  - targets: ['localhost:7000']
	metrics_path: '/metrics'
```

Run Prometheus:
```bash
prometheus --config.file=prometheus.yml
```

Access Prometheus UI: `http://localhost:9090`

## Swagger/OpenAPI Documentation

Access API documentation at: `https://localhost:7000/swagger`

## Activity Tracking & Traceability

Every CRUD operation automatically logs an Activity record with:
- Entity Type (Order, Product, Service)
- Action Type (Create, Update, Delete)
- Changed By (User/System)
- Timestamp
- Old Value & New Value
- Management Icon (success, info, warning, error)

This provides complete audit trail and traceability of all changes.

## Logging

Application uses Serilog for structured logging:
- Logs are written to console
- Configurable log levels per namespace
- Can be extended to write to files, ELK, etc.

## Error Handling

- Global exception handling in controllers
- Metrics tracking for all exceptions
- Structured logging of errors
- Appropriate HTTP status codes returned

## Database Schema

### Tables

**Customers**
- CustomerId (PK)
- Name
- Email
- CreatedDate

**Products**
- ProductId (PK)
- ProductName
- Description
- Price
- CreatedDate

**Services**
- ServiceId (PK)
- ServiceName
- Description
- ServiceType
- Price
- CreatedDate

**Orders**
- OrderId (PK)
- OrderNumber
- CustomerId (FK)
- ProductId (FK)
- ServiceId (FK)
- OrderDate
- Status

**Activities** (Audit Trail)
- ActivityId (PK)
- EntityType
- EntityId
- ActionType
- ChangedBy
- Timestamp
- OldValue
- NewValue
- ManagementIcon
- OrderId (FK)
- ProductId (FK)
- ServiceId (FK)

**CustomerLogs**
- LogId (PK)
- CustomerId (FK)
- OrderId (FK)
- LogMessage
- LogLevel
- Timestamp
- Icon
- Status

## Sample API Requests

### Create Product
```
POST /api/products
Content-Type: application/json

{
  "productName": "Laptop Pro",
  "description": "High-end laptop for professionals",
  "price": 1299.99
}
```

### Create Service
```
POST /api/services
Content-Type: application/json

{
  "serviceName": "Premium Support",
  "description": "24/7 technical support",
  "serviceType": "Support",
  "price": 99.99
}
```

### Create Order
```
POST /api/orders
Content-Type: application/json

{
  "orderNumber": "ORD-001",
  "customerId": 1,
  "productId": 1,
  "serviceId": 1
}
```

### Create Customer Log
```
POST /api/customerlogs
Content-Type: application/json

{
  "customerId": 1,
  "orderId": 1,
  "logMessage": "Customer contacted support about order",
  "logLevel": "Info",
  "icon": "info"
}
```

## Next Steps

1. **Database Setup** - Install PostgreSQL and create database
2. **Configuration** - Update connection strings
3. **Run Migrations** - Create database tables
4. **Start Application** - Run the app
5. **Test APIs** - Use Swagger UI or Postman
6. **Setup Prometheus** - Configure metrics scraping
7. **Monitor** - View metrics in Prometheus UI or Grafana

## Troubleshooting

### Database Connection Issues
- Verify PostgreSQL is running
- Check connection string in appsettings.json
- Ensure database exists and user has permissions

### Migration Issues
```powershell
# Remove last migration if needed
dotnet ef migrations remove

# Recreate migration
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Port Already in Use
```powershell
# Change port in launchSettings.json
# Or specify port when running:
dotnet run --urls="https://localhost:7001"
```

## Support & Documentation

- [Entity Framework Core Docs](https://docs.microsoft.com/ef/)
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core/)
- [Prometheus.Client Documentation](https://github.com/prometheus-net/prometheus-net)
- [Serilog Documentation](https://serilog.net/)

## License

This project is part of the Observability & Traceability solution.
