using Microsoft.EntityFrameworkCore;
using ObeservabiltyTraceabilty.Models;

namespace ObeservabiltyTraceabilty.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<CustomerLog> CustomerLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer Configuration
        modelBuilder.Entity<Customer>()
            .HasKey(c => c.CustomerId);
        modelBuilder.Entity<Customer>()
            .Property(c => c.Name).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Customer>()
            .Property(c => c.Email).IsRequired().HasMaxLength(256);

        // Product Configuration
        modelBuilder.Entity<Product>()
            .HasKey(p => p.ProductId);
        modelBuilder.Entity<Product>()
            .Property(p => p.ProductName).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Product>()
            .Property(p => p.Price).HasPrecision(10, 2);

        // Service Configuration
        modelBuilder.Entity<Service>()
            .HasKey(s => s.ServiceId);
        modelBuilder.Entity<Service>()
            .Property(s => s.ServiceName).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Service>()
            .Property(s => s.Price).HasPrecision(10, 2);

        // Order Configuration
        modelBuilder.Entity<Order>()
            .HasKey(o => o.OrderId);
        modelBuilder.Entity<Order>()
            .Property(o => o.OrderNumber).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Product)
            .WithMany(p => p.Orders)
            .HasForeignKey(o => o.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Service)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Activity Configuration
        modelBuilder.Entity<Activity>()
            .HasKey(a => a.ActivityId);
        modelBuilder.Entity<Activity>()
            .HasOne(a => a.Order)
            .WithMany(o => o.Activities)
            .HasForeignKey(a => a.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Activity>()
            .HasOne(a => a.Product)
            .WithMany(p => p.Activities)
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Activity>()
            .HasOne(a => a.Service)
            .WithMany(s => s.Activities)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.SetNull);

        // CustomerLog Configuration
        modelBuilder.Entity<CustomerLog>()
            .HasKey(cl => cl.LogId);
        modelBuilder.Entity<CustomerLog>()
            .HasOne(cl => cl.Customer)
            .WithMany(c => c.CustomerLogs)
            .HasForeignKey(cl => cl.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CustomerLog>()
            .HasOne(cl => cl.Order)
            .WithMany(o => o.CustomerLogs)
            .HasForeignKey(cl => cl.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
