using ObeservabiltyTraceabilty.Models;
using ObeservabiltyTraceabilty.Repositories;

namespace ObeservabiltyTraceabilty.Services;

public interface IServiceService
{
    Task<Service?> GetServiceByIdAsync(int id);
    Task<IEnumerable<Service>> GetAllServicesAsync();
    Task<Service> CreateServiceAsync(Service service, string createdBy);
    Task<Service> UpdateServiceAsync(Service service, string updatedBy);
    Task DeleteServiceAsync(int id, string deletedBy);
}

public class ServiceService : IServiceService
{
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<Activity> _activityRepository;

    public ServiceService(IRepository<Service> serviceRepository, IRepository<Activity> activityRepository)
    {
        _serviceRepository = serviceRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Service?> GetServiceByIdAsync(int id)
    {
        return await _serviceRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Service>> GetAllServicesAsync()
    {
        return await _serviceRepository.GetAllAsync();
    }

    public async Task<Service> CreateServiceAsync(Service service, string createdBy)
    {
        var createdService = await _serviceRepository.AddAsync(service);

        MetricsService.ServicesCreated.Inc();

        await _activityRepository.AddAsync(new Activity
        {
            EntityType = "Service",
            EntityId = createdService.ServiceId,
            ActionType = "Create",
            ChangedBy = createdBy,
            Timestamp = DateTime.UtcNow,
            NewValue = $"Service {createdService.ServiceName} created",
            ManagementIcon = "success",
            ServiceId = createdService.ServiceId
        });

        return createdService;
    }

    public async Task<Service> UpdateServiceAsync(Service service, string updatedBy)
    {
        var oldService = await _serviceRepository.GetByIdAsync(service.ServiceId);
        var updatedService = await _serviceRepository.UpdateAsync(service);

        MetricsService.ServicesUpdated.Inc();

        await _activityRepository.AddAsync(new Activity
        {
            EntityType = "Service",
            EntityId = updatedService.ServiceId,
            ActionType = "Update",
            ChangedBy = updatedBy,
            Timestamp = DateTime.UtcNow,
            OldValue = oldService?.ServiceName,
            NewValue = $"Service {updatedService.ServiceName} updated",
            ManagementIcon = "info",
            ServiceId = updatedService.ServiceId
        });

        return updatedService;
    }

    public async Task DeleteServiceAsync(int id, string deletedBy)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service != null)
        {
            await _serviceRepository.DeleteAsync(id);

            MetricsService.ServicesDeleted.Inc();

            await _activityRepository.AddAsync(new Activity
            {
                EntityType = "Service",
                EntityId = id,
                ActionType = "Delete",
                ChangedBy = deletedBy,
                Timestamp = DateTime.UtcNow,
                OldValue = $"Service {service.ServiceName}",
                ManagementIcon = "warning",
                ServiceId = id
            });
        }
    }
}
