/**
 * BTHL CheckGate - Kubernetes Metrics Data Models
 * File: src/BTHLCheckGate.Models/KubernetesModels.cs
 * 
 * We define our data models for Kubernetes cluster monitoring and container insights.
 * Our models provide comprehensive cluster information for enterprise monitoring.
 * 
 * @author David St John <davestj@gmail.com>
 * @version 1.0.0
 * @since 2025-09-09
 * 
 * CHANGELOG:
 * 2025-09-09 - FEAT: Initial Kubernetes metrics models for cluster monitoring
 */

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTHLCheckGate.Models
{
    /// <summary>
    /// We represent comprehensive Kubernetes cluster metrics for monitoring dashboards.
    /// Our model captures essential cluster health and resource utilization data.
    /// </summary>
    public class KubernetesClusterMetrics
    {
        /// <summary>
        /// We track when these cluster metrics were collected
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// We identify the cluster being monitored
        /// </summary>
        [Required]
        public string ClusterName { get; set; } = string.Empty;

        /// <summary>
        /// We provide overall cluster status and health information
        /// </summary>
        [Required]
        public KubernetesClusterStatus Status { get; set; } = new();

        /// <summary>
        /// We list all nodes in the cluster with their current status
        /// </summary>
        [Required]
        public List<KubernetesNode> Nodes { get; set; } = new();

        /// <summary>
        /// We track all pods across the cluster with resource utilization
        /// </summary>
        [Required]
        public List<KubernetesPod> Pods { get; set; } = new();

        /// <summary>
        /// We monitor namespace resource allocation and usage
        /// </summary>
        [Required]
        public List<KubernetesNamespace> Namespaces { get; set; } = new();
    }

    /// <summary>
    /// We model the overall health and status of a Kubernetes cluster.
    /// Our design provides essential cluster information for operational monitoring.
    /// </summary>
    public class KubernetesClusterStatus
    {
        /// <summary>
        /// We identify the cluster by its configured name
        /// </summary>
        [Required]
        public string ClusterName { get; set; } = string.Empty;

        /// <summary>
        /// We report the Kubernetes version running on the cluster
        /// </summary>
        [Required]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// We indicate the overall health status of the cluster
        /// </summary>
        [Required]
        public ClusterHealthStatus HealthStatus { get; set; }

        /// <summary>
        /// We count the total number of nodes in the cluster
        /// </summary>
        public int TotalNodes { get; set; }

        /// <summary>
        /// We track the number of nodes that are ready for scheduling
        /// </summary>
        public int ReadyNodes { get; set; }

        /// <summary>
        /// We count the total number of pods across all namespaces
        /// </summary>
        public int TotalPods { get; set; }

        /// <summary>
        /// We track the number of pods in running state
        /// </summary>
        public int RunningPods { get; set; }

        /// <summary>
        /// We monitor the number of pods in pending state
        /// </summary>
        public int PendingPods { get; set; }

        /// <summary>
        /// We count the number of pods in failed state
        /// </summary>
        public int FailedPods { get; set; }

        /// <summary>
        /// We calculate overall CPU utilization across the cluster
        /// </summary>
        [Range(0, 100)]
        public double CpuUtilizationPercent { get; set; }

        /// <summary>
        /// We calculate overall memory utilization across the cluster
        /// </summary>
        [Range(0, 100)]
        public double MemoryUtilizationPercent { get; set; }
    }

    /// <summary>
    /// We represent individual Kubernetes node information and resource status.
    /// Our model provides detailed node health and capacity information.
    /// </summary>
    public class KubernetesNode
    {
        /// <summary>
        /// We identify the node by its unique name
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// We report the current status of the node
        /// </summary>
        [Required]
        public NodeStatus Status { get; set; }

        /// <summary>
        /// We track node roles (master, worker, etc.)
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// We report the Kubernetes version running on this node
        /// </summary>
        public string KubernetesVersion { get; set; } = string.Empty;

        /// <summary>
        /// We track the operating system running on the node
        /// </summary>
        public string OperatingSystem { get; set; } = string.Empty;

        /// <summary>
        /// We monitor CPU capacity available on the node
        /// </summary>
        public string CpuCapacity { get; set; } = string.Empty;

        /// <summary>
        /// We track memory capacity available on the node
        /// </summary>
        public string MemoryCapacity { get; set; } = string.Empty;

        /// <summary>
        /// We calculate current CPU utilization on the node
        /// </summary>
        [Range(0, 100)]
        public double CpuUtilizationPercent { get; set; }

        /// <summary>
        /// We calculate current memory utilization on the node
        /// </summary>
        [Range(0, 100)]
        public double MemoryUtilizationPercent { get; set; }

        /// <summary>
        /// We count the number of pods currently running on this node
        /// </summary>
        public int PodCount { get; set; }
    }

    /// <summary>
    /// We model individual Kubernetes pod information and resource consumption.
    /// Our structure provides detailed workload monitoring capabilities.
    /// </summary>
    public class KubernetesPod
    {
        /// <summary>
        /// We identify the pod by its unique name
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// We identify the namespace containing this pod
        /// </summary>
        [Required]
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// We report the current status of the pod
        /// </summary>
        [Required]
        public PodStatus Status { get; set; }

        /// <summary>
        /// We track which node is hosting this pod
        /// </summary>
        public string NodeName { get; set; } = string.Empty;

        /// <summary>
        /// We record when this pod was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// We track the number of containers in this pod
        /// </summary>
        public int ContainerCount { get; set; }

        /// <summary>
        /// We monitor the number of containers that are ready
        /// </summary>
        public int ReadyContainerCount { get; set; }

        /// <summary>
        /// We count the number of pod restarts
        /// </summary>
        public int RestartCount { get; set; }

        /// <summary>
        /// We track CPU usage for this pod
        /// </summary>
        public string CpuUsage { get; set; } = string.Empty;

        /// <summary>
        /// We monitor memory usage for this pod
        /// </summary>
        public string MemoryUsage { get; set; } = string.Empty;

        /// <summary>
        /// We provide labels attached to this pod
        /// </summary>
        public Dictionary<string, string> Labels { get; set; } = new();
    }

    /// <summary>
    /// We represent Kubernetes namespace information and resource allocation.
    /// Our model provides namespace-level resource governance insights.
    /// </summary>
    public class KubernetesNamespace
    {
        /// <summary>
        /// We identify the namespace by its unique name
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// We report the current status of the namespace
        /// </summary>
        [Required]
        public NamespaceStatus Status { get; set; }

        /// <summary>
        /// We record when this namespace was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// We count the number of pods in this namespace
        /// </summary>
        public int PodCount { get; set; }

        /// <summary>
        /// We track CPU resource quotas for this namespace
        /// </summary>
        public string? CpuQuota { get; set; }

        /// <summary>
        /// We monitor memory resource quotas for this namespace
        /// </summary>
        public string? MemoryQuota { get; set; }

        /// <summary>
        /// We calculate current CPU usage in this namespace
        /// </summary>
        public string CpuUsage { get; set; } = string.Empty;

        /// <summary>
        /// We track current memory usage in this namespace
        /// </summary>
        public string MemoryUsage { get; set; } = string.Empty;
    }

    /// <summary>
    /// We model Kubernetes cluster events for troubleshooting and monitoring.
    /// Our structure provides essential event information for operational insights.
    /// </summary>
    public class KubernetesEvent
    {
        /// <summary>
        /// We record when this event occurred
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// We classify the event type (Normal, Warning, Error)
        /// </summary>
        [Required]
        public EventType Type { get; set; }

        /// <summary>
        /// We provide a brief reason for the event
        /// </summary>
        [Required]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// We include a detailed message about the event
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// We identify the object that generated this event
        /// </summary>
        public string? ObjectName { get; set; }

        /// <summary>
        /// We specify the type of object that generated this event
        /// </summary>
        public string? ObjectKind { get; set; }

        /// <summary>
        /// We identify the namespace where this event occurred
        /// </summary>
        public string? Namespace { get; set; }

        /// <summary>
        /// We track how many times this event has occurred
        /// </summary>
        public int Count { get; set; } = 1;
    }

    // Enumerations for status tracking

    /// <summary>
    /// We define cluster health status levels for monitoring dashboards
    /// </summary>
    public enum ClusterHealthStatus
    {
        Healthy,
        Warning,
        Critical,
        Unknown
    }

    /// <summary>
    /// We define node status values for infrastructure monitoring
    /// </summary>
    public enum NodeStatus
    {
        Ready,
        NotReady,
        Unknown,
        SchedulingDisabled
    }

    /// <summary>
    /// We define pod status values for workload monitoring
    /// </summary>
    public enum PodStatus
    {
        Pending,
        Running,
        Succeeded,
        Failed,
        Unknown
    }

    /// <summary>
    /// We define namespace status values for resource governance
    /// </summary>
    public enum NamespaceStatus
    {
        Active,
        Terminating,
        Unknown
    }

    /// <summary>
    /// We define event types for cluster activity monitoring
    /// </summary>
    public enum EventType
    {
        Normal,
        Warning,
        Error
    }
}