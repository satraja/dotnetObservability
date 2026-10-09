using Microsoft.AspNetCore.Mvc;
using ObeservabiltyTraceabilty.DTOs;
using ObeservabiltyTraceabilty.Extensions;
using ObeservabiltyTraceabilty.Models;
using ObeservabiltyTraceabilty.Services;
using System.Diagnostics;

namespace ObeservabiltyTraceabilty.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _serviceService;
    private readonly ILogger<ServicesController> _logger;

    public ServicesController(IServiceService serviceService, ILogger<ServicesController> logger)
    {
        _serviceService = serviceService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> GetAllServices()
    {
        var traceId = HttpContext.GetTraceId();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("GetAllServices started - TraceId: {TraceId}", traceId);
            var services = await _serviceService.GetAllServicesAsync();
            var dtos = services.Select(s => MapToDto(s)).ToList();
            stopwatch.Stop();
            MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Retrieved {Count} services - TraceId: {TraceId}", dtos.Count, traceId);
            return Ok(new { success = true, data = dtos, traceId = traceId, count = dtos.Count });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error retrieving services - TraceId: {TraceId}", traceId);
            MetricsService.ApiExceptions.WithLabels("GetAllServices", "500").Inc();
            return StatusCode(500, new { success = false, message = "Internal server error", traceId = traceId });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceDto>> GetServiceById(int id)
    {
        var traceId = HttpContext.GetTraceId();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("GetServiceById started for ServiceId: {Id} - TraceId: {TraceId}", id, traceId);
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
            {
                stopwatch.Stop();
                MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
                return NotFound($"Service with id {id} not found");
            }
            stopwatch.Stop();
            MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            return Ok(MapToDto(service));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error retrieving service {Id}", id);
            MetricsService.ApiExceptions.WithLabels("GetServiceById", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ServiceDto>> CreateService([FromBody] CreateServiceDto dto)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var service = new Service
            {
                ServiceName = dto.ServiceName,
                Description = dto.Description,
                ServiceType = dto.ServiceType,
                Price = dto.Price,
                CreatedDate = DateTime.UtcNow
            };

            var createdService = await _serviceService.CreateServiceAsync(service, "System");
            stopwatch.Stop();
            MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Service {ServiceName} created successfully", createdService.ServiceName);
            return CreatedAtAction(nameof(GetServiceById), new { id = createdService.ServiceId }, MapToDto(createdService));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error creating service");
            MetricsService.ApiExceptions.WithLabels("CreateService", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServiceDto>> UpdateService(int id, [FromBody] UpdateServiceDto dto)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
            {
                stopwatch.Stop();
                MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
                return NotFound($"Service with id {id} not found");
            }

            if (!string.IsNullOrEmpty(dto.ServiceName))
                service.ServiceName = dto.ServiceName;
            if (!string.IsNullOrEmpty(dto.Description))
                service.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.ServiceType))
                service.ServiceType = dto.ServiceType;
            if (dto.Price.HasValue && dto.Price > 0)
                service.Price = dto.Price.Value;

            var updatedService = await _serviceService.UpdateServiceAsync(service, "System");
            stopwatch.Stop();
            MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Service {Id} updated successfully", id);
            return Ok(MapToDto(updatedService));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error updating service {Id}", id);
            MetricsService.ApiExceptions.WithLabels("UpdateService", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
            {
                stopwatch.Stop();
                MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
                return NotFound($"Service with id {id} not found");
            }

            await _serviceService.DeleteServiceAsync(id, "System");
            stopwatch.Stop();
            MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Service {Id} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.ServiceApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error deleting service {Id}", id);
            MetricsService.ApiExceptions.WithLabels("DeleteService", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    private static ServiceDto MapToDto(Service service)
    {
        return new ServiceDto
        {
            ServiceId = service.ServiceId,
            ServiceName = service.ServiceName,
            Description = service.Description,
            ServiceType = service.ServiceType,
            Price = service.Price,
            CreatedDate = service.CreatedDate
        };
    }
}
