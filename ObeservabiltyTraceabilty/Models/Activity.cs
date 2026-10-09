namespace ObeservabiltyTraceabilty.Models;

public class Activity
{
    public int ActivityId { get; set; }
    public string EntityType { get; set; } = string.Empty; // Order, Product, Service
    public int EntityId { get; set; }
    public string ActionType { get; set; } = string.Empty; // Create, Update, Delete
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ManagementIcon { get; set; } = "info"; // info, warning, success, error

    public int? OrderId { get; set; }
    public Order? Order { get; set; }
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public int? ServiceId { get; set; }
    public Service? Service { get; set; }
}
