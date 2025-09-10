/**
 * BTHL CheckGate - System Metrics Repository Interface
 * File: src/BTHLCheckGate.Data/Repositories/ISystemMetricsRepository.cs
 */

using BTHLCheckGate.Models;

namespace BTHLCheckGate.Data.Repositories
{
    public interface ISystemMetricsRepository
    {
        Task<SystemMetrics?> GetLatestMetricsAsync();
        Task<PagedResult<SystemMetrics>> GetHistoricalMetricsAsync(DateTime startTime, DateTime endTime, int intervalMinutes, int page, int pageSize);
        Task<SystemMetricsSummary> GetMetricsSummaryAsync(DateTime startTime, DateTime endTime);
        Task<List<SystemAlert>> GetActiveAlertsAsync(AlertSeverity? severity, int limit);
        Task StoreSystemMetricsAsync(SystemMetrics metrics);
    }
}