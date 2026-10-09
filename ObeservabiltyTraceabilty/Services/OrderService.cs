using ObeservabiltyTraceabilty.Models;
using ObeservabiltyTraceabilty.Repositories;

namespace ObeservabiltyTraceabilty.Services;

public interface IOrderService
{
    Task<Order?> GetOrderByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<Order> CreateOrderAsync(Order order, string createdBy);
    Task<Order> UpdateOrderAsync(Order order, string updatedBy);
    Task DeleteOrderAsync(int id, string deletedBy);
}

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Activity> _activityRepository;

    public OrderService(IRepository<Order> orderRepository, IRepository<Activity> activityRepository)
    {
        _orderRepository = orderRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _orderRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _orderRepository.GetAllAsync();
    }

    public async Task<Order> CreateOrderAsync(Order order, string createdBy)
    {
        var createdOrder = await _orderRepository.AddAsync(order);

        MetricsService.OrdersCreated.Inc();

        await _activityRepository.AddAsync(new Activity
        {
            EntityType = "Order",
            EntityId = createdOrder.OrderId,
            ActionType = "Create",
            ChangedBy = createdBy,
            Timestamp = DateTime.UtcNow,
            NewValue = $"Order {createdOrder.OrderNumber} created",
            ManagementIcon = "success",
            OrderId = createdOrder.OrderId
        });

        return createdOrder;
    }

    public async Task<Order> UpdateOrderAsync(Order order, string updatedBy)
    {
        var oldOrder = await _orderRepository.GetByIdAsync(order.OrderId);
        var updatedOrder = await _orderRepository.UpdateAsync(order);

        MetricsService.OrdersUpdated.Inc();

        await _activityRepository.AddAsync(new Activity
        {
            EntityType = "Order",
            EntityId = updatedOrder.OrderId,
            ActionType = "Update",
            ChangedBy = updatedBy,
            Timestamp = DateTime.UtcNow,
            OldValue = oldOrder?.Status,
            NewValue = $"Order status updated to {updatedOrder.Status}",
            ManagementIcon = "info",
            OrderId = updatedOrder.OrderId
        });

        return updatedOrder;
    }

    public async Task DeleteOrderAsync(int id, string deletedBy)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order != null)
        {
            await _orderRepository.DeleteAsync(id);

            MetricsService.OrdersDeleted.Inc();

            await _activityRepository.AddAsync(new Activity
            {
                EntityType = "Order",
                EntityId = id,
                ActionType = "Delete",
                ChangedBy = deletedBy,
                Timestamp = DateTime.UtcNow,
                OldValue = $"Order {order.OrderNumber}",
                ManagementIcon = "warning",
                OrderId = id
            });
        }
    }
}
