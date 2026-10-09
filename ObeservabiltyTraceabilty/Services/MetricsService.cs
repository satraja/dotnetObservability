using Prometheus;

namespace ObeservabiltyTraceabilty.Services;

public class MetricsService
{
    // Counters
    public static readonly Counter OrdersCreated = Metrics.CreateCounter(
        "orders_created_total",
        "Total orders created");

    public static readonly Counter OrdersUpdated = Metrics.CreateCounter(
        "orders_updated_total",
        "Total orders updated");

    public static readonly Counter OrdersDeleted = Metrics.CreateCounter(
        "orders_deleted_total",
        "Total orders deleted");

    public static readonly Counter ProductsCreated = Metrics.CreateCounter(
        "products_created_total",
        "Total products created");

    public static readonly Counter ProductsUpdated = Metrics.CreateCounter(
        "products_updated_total",
        "Total products updated");

    public static readonly Counter ProductsDeleted = Metrics.CreateCounter(
        "products_deleted_total",
        "Total products deleted");

    public static readonly Counter ServicesCreated = Metrics.CreateCounter(
        "services_created_total",
        "Total services created");

    public static readonly Counter ServicesUpdated = Metrics.CreateCounter(
        "services_updated_total",
        "Total services updated");

    public static readonly Counter ServicesDeleted = Metrics.CreateCounter(
        "services_deleted_total",
        "Total services deleted");

    public static readonly Counter ActivitiesLogged = Metrics.CreateCounter(
        "activities_logged_total",
        "Total activities logged");

    public static readonly Counter ApiExceptions = Metrics.CreateCounter(
        "api_exceptions_total",
        "Total API exceptions",
        new CounterConfiguration { LabelNames = new[] { "endpoint", "status" } });

    // Gauges
    public static readonly Gauge ActiveOrdersCount = Metrics.CreateGauge(
        "active_orders_count",
        "Current active orders");

    public static readonly Gauge ActiveServicesCount = Metrics.CreateGauge(
        "active_services_count",
        "Current active services");

    public static readonly Gauge DatabaseConnectionPoolSize = Metrics.CreateGauge(
        "database_connection_pool_size",
        "Database connection pool status");

    // Histograms
    public static readonly Histogram OrderApiDuration = Metrics.CreateHistogram(
        "order_api_duration_seconds",
        "Order API response time in seconds",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) });

    public static readonly Histogram ProductApiDuration = Metrics.CreateHistogram(
        "product_api_duration_seconds",
        "Product API response time in seconds",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) });

    public static readonly Histogram ServiceApiDuration = Metrics.CreateHistogram(
        "service_api_duration_seconds",
        "Service API response time in seconds",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) });

    public static readonly Histogram DatabaseQueryDuration = Metrics.CreateHistogram(
        "database_query_duration_seconds",
        "Database query latency in seconds",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) });
}
