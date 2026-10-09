using ObeservabiltyTraceabilty.Models;
using ObeservabiltyTraceabilty.Repositories;

namespace ObeservabiltyTraceabilty.Services;

public interface IActivityService
{
    Task<Activity?> GetActivityByIdAsync(int id);
    Task<IEnumerable<Activity>> GetAllActivitiesAsync();
    Task<IEnumerable<Activity>> GetActivitiesByEntityAsync(string entityType, int entityId);
    Task<Activity> LogActivityAsync(Activity activity);
}

public class ActivityService : IActivityService
{
    private readonly IRepository<Activity> _activityRepository;

    public ActivityService(IRepository<Activity> activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<Activity?> GetActivityByIdAsync(int id)
    {
        return await _activityRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Activity>> GetAllActivitiesAsync()
    {
        var allActivities = await _activityRepository.GetAllAsync();
        return allActivities.OrderByDescending(a => a.Timestamp);
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByEntityAsync(string entityType, int entityId)
    {
        var allActivities = await _activityRepository.GetAllAsync();
        return allActivities
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp);
    }

    public async Task<Activity> LogActivityAsync(Activity activity)
    {
        var loggedActivity = await _activityRepository.AddAsync(activity);
        MetricsService.ActivitiesLogged.Inc();
        return loggedActivity;
    }
}
