namespace ObeservabiltyTraceabilty.Models;

public class CustomerLog
{
    public int LogId { get; set; }
    public int CustomerId { get; set; }
    public int? OrderId { get; set; }
    public string LogMessage { get; set; } = string.Empty;
    public string LogLevel { get; set; } = "Info"; // Info, Warning, Error
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Icon { get; set; } = "info"; // info, warning, success, error
    public string Status { get; set; } = "Active";

    public Customer? Customer { get; set; }
    public Order? Order { get; set; }
}
