namespace ObeservabiltyTraceabilty.DTOs;

public class OrderDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int ServiceId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int ServiceId { get; set; }
}

public class UpdateOrderDto
{
    public string? Status { get; set; }
}

public class ProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class UpdateProductDto
{
    public string? ProductName { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
}

public class ServiceDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateServiceDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class UpdateServiceDto
{
    public string? ServiceName { get; set; }
    public string? Description { get; set; }
    public string? ServiceType { get; set; }
    public decimal? Price { get; set; }
}

public class ActivityDto
{
    public int ActivityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ManagementIcon { get; set; } = string.Empty;
}

public class CustomerLogDto
{
    public int LogId { get; set; }
    public int CustomerId { get; set; }
    public int? OrderId { get; set; }
    public string LogMessage { get; set; } = string.Empty;
    public string LogLevel { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class CreateCustomerLogDto
{
    public int CustomerId { get; set; }
    public int? OrderId { get; set; }
    public string LogMessage { get; set; } = string.Empty;
    public string LogLevel { get; set; } = "Info";
    public string Icon { get; set; } = "info";
}
