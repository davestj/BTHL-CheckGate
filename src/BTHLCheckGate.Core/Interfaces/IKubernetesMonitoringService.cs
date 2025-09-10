/**
 * BTHL CheckGate - Kubernetes Monitoring Service Interface
 * File: src/BTHLCheckGate.Core/Interfaces/IKubernetesMonitoringService.cs
 * 
 * We define our contract for Kubernetes cluster monitoring capabilities.
 * Our interface enables comprehensive container orchestration insights and health monitoring.
 * 
 * @author David St John <davestj@gmail.com>
 * @version 1.0.0
 * @since 2025-09-09
 * 
 * CHANGELOG:
 * 2025-09-09 - FEAT: Initial Kubernetes monitoring service interface
 */

using BTHLCheckGate.Models;

namespace BTHLCheckGate.Core.Interfaces
{
    /// <summary>
    /// We provide comprehensive Kubernetes cluster monitoring through this service interface.
    /// Our design enables real-time container workload insights and cluster health assessment.
    /// </summary>
    public interface IKubernetesMonitoringService
    {
        /// <summary>
        /// We collect comprehensive cluster metrics including node status and resource utilization.
        /// Our implementation provides real-time cluster health data for monitoring dashboards.
        /// </summary>
        /// <returns>Complete cluster metrics and status information</returns>
        Task<KubernetesClusterMetrics> CollectClusterMetricsAsync();

        /// <summary>
        /// We retrieve current cluster status including node health and resource availability.
        /// Our method provides essential cluster information for operational monitoring.
        /// </summary>
        /// <param name="clusterName">Optional cluster name filter for multi-cluster environments</param>
        /// <returns>Current cluster status and health information</returns>
        Task<KubernetesClusterStatus> GetClusterStatusAsync(string? clusterName = null);

        /// <summary>
        /// We gather detailed information about all pods across the cluster.
        /// Our implementation provides workload status and resource consumption data.
        /// </summary>
        /// <param name="namespace">Optional namespace filter for targeted monitoring</param>
        /// <returns>Collection of pod information with resource metrics</returns>
        Task<List<KubernetesPod>> GetPodsAsync(string? @namespace = null);

        /// <summary>
        /// We monitor node health and resource utilization across the cluster.
        /// Our method provides essential infrastructure health data for capacity planning.
        /// </summary>
        /// <returns>Node status and resource utilization information</returns>
        Task<List<KubernetesNode>> GetNodesAsync();

        /// <summary>
        /// We track namespace resource allocation and consumption patterns.
        /// Our implementation enables resource governance and utilization analysis.
        /// </summary>
        /// <returns>Namespace resource utilization data</returns>
        Task<List<KubernetesNamespace>> GetNamespacesAsync();

        /// <summary>
        /// We analyze cluster events for troubleshooting and operational insights.
        /// Our method provides recent cluster activity for issue identification.
        /// </summary>
        /// <param name="hours">Number of hours to look back for events</param>
        /// <returns>Recent cluster events with severity classification</returns>
        Task<List<KubernetesEvent>> GetRecentEventsAsync(int hours = 24);

        /// <summary>
        /// We validate cluster health against best practices and alerting thresholds.
        /// Our implementation provides immediate notification of cluster issues.
        /// </summary>
        /// <returns>Collection of active cluster health alerts</returns>
        Task<List<SystemAlert>> CheckClusterHealthAsync();
    }
}