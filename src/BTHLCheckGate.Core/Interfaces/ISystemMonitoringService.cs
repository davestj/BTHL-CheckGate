/**
 * BTHL CheckGate - System Monitoring Service Interface
 * File: src/BTHLCheckGate.Core/Interfaces/ISystemMonitoringService.cs
 * 
 * We define our contract for system resource monitoring capabilities.
 * Our interface provides comprehensive system metrics collection with async operations.
 * 
 * @author David St John <davestj@gmail.com>
 * @version 1.0.0
 * @since 2025-09-09
 * 
 * CHANGELOG:
 * 2025-09-09 - FEAT: Initial system monitoring service interface
 */

using BTHLCheckGate.Models;

namespace BTHLCheckGate.Core.Interfaces
{
    /// <summary>
    /// We provide comprehensive system resource monitoring through this service interface.
    /// Our design enables real-time metrics collection while maintaining performance efficiency.
    /// </summary>
    public interface ISystemMonitoringService
    {
        /// <summary>
        /// We collect current system metrics including CPU, memory, disk, and network statistics.
        /// Our implementation provides detailed resource utilization data for monitoring dashboards.
        /// </summary>
        /// <returns>Complete system metrics data structure</returns>
        Task<SystemMetrics> CollectSystemMetricsAsync();

        /// <summary>
        /// We retrieve CPU utilization across all cores and logical processors.
        /// Our method provides both overall and per-core utilization percentages.
        /// </summary>
        /// <returns>Detailed CPU performance metrics</returns>
        Task<CpuMetrics> GetCpuMetricsAsync();

        /// <summary>
        /// We gather memory usage statistics including physical and virtual memory consumption.
        /// Our implementation tracks both total capacity and current utilization patterns.
        /// </summary>
        /// <returns>Comprehensive memory utilization data</returns>
        Task<MemoryMetrics> GetMemoryMetricsAsync();

        /// <summary>
        /// We monitor all disk drives for capacity and performance characteristics.
        /// Our method provides per-drive statistics including space usage and I/O operations.
        /// </summary>
        /// <returns>List of disk metrics for all available drives</returns>
        Task<List<DiskMetrics>> GetDiskMetricsAsync();

        /// <summary>
        /// We track network interface statistics and throughput information.
        /// Our implementation captures bandwidth utilization and error rates per interface.
        /// </summary>
        /// <returns>Network interface performance data</returns>
        Task<List<NetworkMetrics>> GetNetworkMetricsAsync();

        /// <summary>
        /// We analyze running processes for resource consumption patterns.
        /// Our method identifies top CPU and memory consumers for performance analysis.
        /// </summary>
        /// <returns>Process metrics with resource utilization rankings</returns>
        Task<ProcessMetrics> GetProcessMetricsAsync();

        /// <summary>
        /// We validate system health against configured thresholds and alerting rules.
        /// Our implementation provides immediate notification of resource constraints.
        /// </summary>
        /// <returns>Collection of active system health alerts</returns>
        Task<List<SystemAlert>> CheckSystemHealthAsync();
    }
}