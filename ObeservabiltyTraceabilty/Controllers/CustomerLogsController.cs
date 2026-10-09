using Microsoft.AspNetCore.Mvc;
using ObeservabiltyTraceabilty.DTOs;
using ObeservabiltyTraceabilty.Models;
using ObeservabiltyTraceabilty.Services;

namespace ObeservabiltyTraceabilty.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerLogsController : ControllerBase
{
    private readonly ICustomerLogService _logService;
    private readonly ILogger<CustomerLogsController> _logger;

    public CustomerLogsController(ICustomerLogService logService, ILogger<CustomerLogsController> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerLogDto>>> GetAllLogs()
    {
        try
        {
            var logs = await _logService.GetAllLogsAsync();
            var dtos = logs.Select(l => MapToDto(l)).ToList();
            _logger.LogInformation("Retrieved {Count} customer logs", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer logs");
            MetricsService.ApiExceptions.WithLabels("GetAllLogs", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerLogDto>> GetLogById(int id)
    {
        try
        {
            var log = await _logService.GetLogByIdAsync(id);
            if (log == null)
            {
                return NotFound($"Log with id {id} not found");
            }
            return Ok(MapToDto(log));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving log {Id}", id);
            MetricsService.ApiExceptions.WithLabels("GetLogById", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<CustomerLogDto>>> GetLogsByCustomer(int customerId)
    {
        try
        {
            var logs = await _logService.GetLogsByCustomerAsync(customerId);
            var dtos = logs.Select(l => MapToDto(l)).ToList();
            _logger.LogInformation("Retrieved {Count} logs for customer {CustomerId}", dtos.Count, customerId);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for customer {CustomerId}", customerId);
            MetricsService.ApiExceptions.WithLabels("GetLogsByCustomer", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<IEnumerable<CustomerLogDto>>> GetLogsByOrder(int orderId)
    {
        try
        {
            var logs = await _logService.GetLogsByOrderAsync(orderId);
            var dtos = logs.Select(l => MapToDto(l)).ToList();
            _logger.LogInformation("Retrieved {Count} logs for order {OrderId}", dtos.Count, orderId);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for order {OrderId}", orderId);
            MetricsService.ApiExceptions.WithLabels("GetLogsByOrder", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CustomerLogDto>> CreateLog([FromBody] CreateCustomerLogDto dto)
    {
        try
        {
            var log = new CustomerLog
            {
                CustomerId = dto.CustomerId,
                OrderId = dto.OrderId,
                LogMessage = dto.LogMessage,
                LogLevel = dto.LogLevel,
                Timestamp = DateTime.UtcNow,
                Icon = dto.Icon,
                Status = "Active"
            };

            var createdLog = await _logService.CreateLogAsync(log);
            _logger.LogInformation("Customer log created successfully for customer {CustomerId}", createdLog.CustomerId);
            return CreatedAtAction(nameof(GetLogById), new { id = createdLog.LogId }, MapToDto(createdLog));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer log");
            MetricsService.ApiExceptions.WithLabels("CreateLog", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    private static CustomerLogDto MapToDto(CustomerLog log)
    {
        return new CustomerLogDto
        {
            LogId = log.LogId,
            CustomerId = log.CustomerId,
            OrderId = log.OrderId,
            LogMessage = log.LogMessage,
            LogLevel = log.LogLevel,
            Timestamp = log.Timestamp,
            Icon = log.Icon,
            Status = log.Status
        };
    }
}
