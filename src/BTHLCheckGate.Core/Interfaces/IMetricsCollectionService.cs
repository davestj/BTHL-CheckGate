/**
 * BTHL CheckGate - Metrics Collection Service Interface
 * File: src/BTHLCheckGate.Core/Interfaces/IMetricsCollectionService.cs
 * 
 * We define our contract for centralized metrics collection and storage operations.
 * Our interface coordinates data gathering from multiple monitoring sources.
 * 
 * @author David St John <davestj@gmail.com>
 * @version 1.0.0
 * @since 2025-09-09
 * 
 * CHANGELOG:
 * 2025-09-09 - FEAT: Initial metrics collection service interface
 */

using BTHLCheckGate.Models;

namespace BTHLCheckGate.Core.Interfaces
{
    /// <summary>
    /// We coordinate comprehensive metrics collection from all monitoring sources.
    /// Our design ensures efficient data gathering and persistent storage for analytics.
    /// </summary>
    public interface IMetricsCollectionService
    {
        /// <summary>
        /// We store collected system and cluster metrics in our persistent data store.
        /// Our implementation handles batch processing and data validation.
        /// </summary>
        /// <param name="systemMetrics">System resource metrics to store</param>
        /// <param name="clusterMetrics">Kubernetes cluster metrics to store</param>
        /// <returns>Task representing the storage operation</returns>
        Task StoreMetricsAsync(SystemMetrics systemMetrics, KubernetesClusterMetrics clusterMetrics);

        /// <summary>
        /// We orchestrate a complete metrics collection cycle from all sources.
        /// Our method coordinates system and cluster monitoring for comprehensive coverage.
        /// </summary>
        /// <returns>Task representing the complete collection cycle</returns>
        Task CollectAllMetricsAsync();

        /// <summary>
        /// We clean up historical data based on configured retention policies.
        /// Our implementation maintains optimal database performance while preserving essential trends.
        /// </summary>
        /// <param name="retentionDays">Number of days to retain detailed metrics data</param>
        /// <returns>Task representing the cleanup operation</returns>
        Task CleanupOldMetricsAsync(int retentionDays = 90);

        /// <summary>
        /// We export metrics data in various formats for external analysis tools.
        /// Our method supports CSV, JSON, and other standard data exchange formats.
        /// </summary>
        /// <param name="startTime">Beginning of export time range</param>
        /// <param name="endTime">End of export time range</param>
        /// <param name="format">Export format (CSV, JSON, XML)</param>
        /// <returns>Exported metrics data in the specified format</returns>
        Task<byte[]> ExportMetricsAsync(DateTime startTime, DateTime endTime, string format = "CSV");

        /// <summary>
        /// We calculate performance baselines and trends for capacity planning.
        /// Our implementation provides statistical analysis of resource utilization patterns.
        /// </summary>
        /// <param name="days">Number of days to analyze for baseline calculation</param>
        /// <returns>Performance baseline and trend analysis data</returns>
        Task<PerformanceBaseline> CalculatePerformanceBaselineAsync(int days = 30);
    }
}