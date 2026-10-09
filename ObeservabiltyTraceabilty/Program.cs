using Microsoft.EntityFrameworkCore;
using ObeservabiltyTraceabilty.Data;
using ObeservabiltyTraceabilty.Extensions;
using ObeservabiltyTraceabilty.Logging;
using ObeservabiltyTraceabilty.Middleware;
using ObeservabiltyTraceabilty.Repositories;
using ObeservabiltyTraceabilty.Services;
using Prometheus;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

// Add HttpContextAccessor for accessing trace IDs
builder.Services.AddHttpContextAccessor();

// Configure Serilog with TraceId enricher and HTTP sink for Loki
var httpContextAccessor = new HttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

var lokiUri = builder.Configuration.GetConnectionString("Loki") 
    ?? "http://localhost:3100";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.With(new TraceIdEnricher(httpContextAccessor))
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ObservabilityApp")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] CorrelationId: {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .WriteTo.GrafanaLoki(
        lokiUri,
        labels: new[]
        {
            new LokiLabel { Key = "app", Value = "observability-app" },
            new LokiLabel { Key = "environment", Value = builder.Environment.EnvironmentName }
        })
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection") 
    ?? "Host=localhost;Database=ObservabilityTraceability;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository Pattern
builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

// Business Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<ICustomerLogService, CustomerLogService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
}

// Add Trace ID middleware (early in pipeline)
app.UseMiddleware<TraceIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Expose Prometheus metrics endpoint
app.MapMetrics("/metrics");

app.MapControllers();

try
{
    Log.Information("Starting application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
