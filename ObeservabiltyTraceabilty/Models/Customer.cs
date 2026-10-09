namespace ObeservabiltyTraceabilty.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CustomerLog> CustomerLogs { get; set; } = new List<CustomerLog>();
}
