using ObeservabiltyTraceabilty.Models;
using ObeservabiltyTraceabilty.Repositories;

namespace ObeservabiltyTraceabilty.Services;

public interface ICustomerLogService
{
    Task<CustomerLog?> GetLogByIdAsync(int id);
    Task<IEnumerable<CustomerLog>> GetAllLogsAsync();
    Task<IEnumerable<CustomerLog>> GetLogsByCustomerAsync(int customerId);
    Task<IEnumerable<CustomerLog>> GetLogsByOrderAsync(int orderId);
    Task<CustomerLog> CreateLogAsync(CustomerLog log);
}

public class CustomerLogService : ICustomerLogService
{
    private readonly IRepository<CustomerLog> _logRepository;

    public CustomerLogService(IRepository<CustomerLog> logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<CustomerLog?> GetLogByIdAsync(int id)
    {
        return await _logRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<CustomerLog>> GetAllLogsAsync()
    {
        var allLogs = await _logRepository.GetAllAsync();
        return allLogs.OrderByDescending(l => l.Timestamp);
    }

    public async Task<IEnumerable<CustomerLog>> GetLogsByCustomerAsync(int customerId)
    {
        var allLogs = await _logRepository.GetAllAsync();
        return allLogs
            .Where(l => l.CustomerId == customerId)
            .OrderByDescending(l => l.Timestamp);
    }

    public async Task<IEnumerable<CustomerLog>> GetLogsByOrderAsync(int orderId)
    {
        var allLogs = await _logRepository.GetAllAsync();
        return allLogs
            .Where(l => l.OrderId == orderId)
            .OrderByDescending(l => l.Timestamp);
    }

    public async Task<CustomerLog> CreateLogAsync(CustomerLog log)
    {
        return await _logRepository.AddAsync(log);
    }
}
