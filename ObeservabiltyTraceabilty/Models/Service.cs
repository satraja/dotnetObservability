namespace ObeservabiltyTraceabilty.Models;

public class Service
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
