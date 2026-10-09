namespace ObeservabiltyTraceabilty.Models;

public class Order
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int ServiceId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";

    public Customer? Customer { get; set; }
    public Product? Product { get; set; }
    public Service? Service { get; set; }
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<CustomerLog> CustomerLogs { get; set; } = new List<CustomerLog>();
}
