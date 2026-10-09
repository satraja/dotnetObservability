using Microsoft.AspNetCore.Mvc;
using ObeservabiltyTraceabilty.DTOs;
using ObeservabiltyTraceabilty.Extensions;
using ObeservabiltyTraceabilty.Models;
using ObeservabiltyTraceabilty.Services;
using System.Diagnostics;

namespace ObeservabiltyTraceabilty.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
    {
        var traceId = HttpContext.GetTraceId();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("GetAllOrders started - TraceId: {TraceId}", traceId);
            var orders = await _orderService.GetAllOrdersAsync();
            var dtos = orders.Select(o => MapToDto(o)).ToList();
            stopwatch.Stop();
            MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Retrieved {Count} orders - TraceId: {TraceId}", dtos.Count, traceId);
            return Ok(new { success = true, data = dtos, traceId = traceId, count = dtos.Count });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error retrieving orders - TraceId: {TraceId}", traceId);
            MetricsService.ApiExceptions.WithLabels("GetAllOrders", "500").Inc();
            return StatusCode(500, new { success = false, message = "Internal server error", traceId = traceId });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(int id)
    {
        var traceId = HttpContext.GetTraceId();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("GetOrderById started for OrderId: {Id} - TraceId: {TraceId}", id, traceId);
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                stopwatch.Stop();
                MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
                return NotFound($"Order with id {id} not found");
            }
            stopwatch.Stop();
            MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            return Ok(MapToDto(order));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error retrieving order {Id}", id);
            MetricsService.ApiExceptions.WithLabels("GetOrderById", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var order = new Order
            {
                OrderNumber = dto.OrderNumber,
                CustomerId = dto.CustomerId,
                ProductId = dto.ProductId,
                ServiceId = dto.ServiceId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            };

            var createdOrder = await _orderService.CreateOrderAsync(order, "System");
            stopwatch.Stop();
            MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Order {OrderNumber} created successfully", createdOrder.OrderNumber);
            return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.OrderId }, MapToDto(createdOrder));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error creating order");
            MetricsService.ApiExceptions.WithLabels("CreateOrder", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<OrderDto>> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                stopwatch.Stop();
                MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
                return NotFound($"Order with id {id} not found");
            }

            if (!string.IsNullOrEmpty(dto.Status))
            {
                order.Status = dto.Status;
            }

            var updatedOrder = await _orderService.UpdateOrderAsync(order, "System");
            stopwatch.Stop();
            MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Order {Id} updated successfully", id);
            return Ok(MapToDto(updatedOrder));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error updating order {Id}", id);
            MetricsService.ApiExceptions.WithLabels("UpdateOrder", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                stopwatch.Stop();
                MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
                return NotFound($"Order with id {id} not found");
            }

            await _orderService.DeleteOrderAsync(id, "System");
            stopwatch.Stop();
            MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Order {Id} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.OrderApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error deleting order {Id}", id);
            MetricsService.ApiExceptions.WithLabels("DeleteOrder", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            ServiceId = order.ServiceId,
            OrderDate = order.OrderDate,
            Status = order.Status
        };
    }
}
