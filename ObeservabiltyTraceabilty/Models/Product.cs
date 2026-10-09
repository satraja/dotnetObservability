namespace ObeservabiltyTraceabilty.Models;

public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
