using Microsoft.AspNetCore.Mvc;
using ObeservabiltyTraceabilty.DTOs;
using ObeservabiltyTraceabilty.Services;

namespace ObeservabiltyTraceabilty.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(IActivityService activityService, ILogger<ActivitiesController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetAllActivities()
    {
        try
        {
            var activities = await _activityService.GetAllActivitiesAsync();
            var dtos = activities.Select(a => MapToDto(a)).ToList();
            _logger.LogInformation("Retrieved {Count} activities", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities");
            MetricsService.ApiExceptions.WithLabels("GetAllActivities", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActivityDto>> GetActivityById(int id)
    {
        try
        {
            var activity = await _activityService.GetActivityByIdAsync(id);
            if (activity == null)
            {
                return NotFound($"Activity with id {id} not found");
            }
            return Ok(MapToDto(activity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity {Id}", id);
            MetricsService.ApiExceptions.WithLabels("GetActivityById", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivitiesByEntity(string entityType, int entityId)
    {
        try
        {
            var activities = await _activityService.GetActivitiesByEntityAsync(entityType, entityId);
            var dtos = activities.Select(a => MapToDto(a)).ToList();
            _logger.LogInformation("Retrieved {Count} activities for {EntityType} {EntityId}", dtos.Count, entityType, entityId);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities for entity {EntityType} {EntityId}", entityType, entityId);
            MetricsService.ApiExceptions.WithLabels("GetActivitiesByEntity", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    private static ActivityDto MapToDto(ObeservabiltyTraceabilty.Models.Activity activity)
    {
        return new ActivityDto
        {
            ActivityId = activity.ActivityId,
            EntityType = activity.EntityType,
            EntityId = activity.EntityId,
            ActionType = activity.ActionType,
            ChangedBy = activity.ChangedBy,
            Timestamp = activity.Timestamp,
            OldValue = activity.OldValue,
            NewValue = activity.NewValue,
            ManagementIcon = activity.ManagementIcon
        };
    }
}
