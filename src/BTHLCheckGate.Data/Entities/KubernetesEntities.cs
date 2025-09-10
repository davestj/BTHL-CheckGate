/**
 * BTHL CheckGate - Kubernetes Database Entities
 * File: src/BTHLCheckGate.Data/Entities/KubernetesEntities.cs
 */

using BTHLCheckGate.Models;

namespace BTHLCheckGate.Data
{
    public class KubernetesClusterMetricsEntity
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ClusterName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public ClusterHealthStatus HealthStatus { get; set; }
        public int TotalNodes { get; set; }
        public int ReadyNodes { get; set; }
        public int TotalPods { get; set; }
        public int RunningPods { get; set; }
        public int PendingPods { get; set; }
        public int FailedPods { get; set; }
        public double CpuUtilizationPercent { get; set; }
        public double MemoryUtilizationPercent { get; set; }
    }

    public class KubernetesNodeEntity
    {
        public int Id { get; set; }
        public int ClusterMetricsId { get; set; }
        public string Name { get; set; } = string.Empty;
        public NodeStatus Status { get; set; }
        public string Roles { get; set; } = "[]"; // JSON array
        public string KubernetesVersion { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string CpuCapacity { get; set; } = string.Empty;
        public string MemoryCapacity { get; set; } = string.Empty;
        public double CpuUtilizationPercent { get; set; }
        public double MemoryUtilizationPercent { get; set; }
        public int PodCount { get; set; }
    }

    public class KubernetesPodEntity
    {
        public int Id { get; set; }
        public int ClusterMetricsId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public PodStatus Status { get; set; }
        public string NodeName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ContainerCount { get; set; }
        public int ReadyContainerCount { get; set; }
        public int RestartCount { get; set; }
        public string CpuUsage { get; set; } = string.Empty;
        public string MemoryUsage { get; set; } = string.Empty;
        public string Labels { get; set; } = "{}"; // JSON object
    }

    public class KubernetesNamespaceEntity
    {
        public int Id { get; set; }
        public int ClusterMetricsId { get; set; }
        public string Name { get; set; } = string.Empty;
        public NamespaceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PodCount { get; set; }
        public string? CpuQuota { get; set; }
        public string? MemoryQuota { get; set; }
        public string CpuUsage { get; set; } = string.Empty;
        public string MemoryUsage { get; set; } = string.Empty;
    }

    public class KubernetesEventEntity
    {
        public int Id { get; set; }
        public int ClusterMetricsId { get; set; }
        public DateTime Timestamp { get; set; }
        public EventType Type { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ObjectName { get; set; }
        public string? ObjectKind { get; set; }
        public string? Namespace { get; set; }
        public int Count { get; set; } = 1;
    }

    public class SystemAlertEntity
    {
        public string Id { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public AlertSeverity Severity { get; set; }
        public AlertType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string? Hostname { get; set; }
        public AlertStatus Status { get; set; }
        public double? TriggerValue { get; set; }
        public double? ThresholdValue { get; set; }
        public string? Unit { get; set; }
        public string Metadata { get; set; } = "{}"; // JSON
        public DateTime? AcknowledgedAt { get; set; }
        public string? AcknowledgedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class ApiTokenEntity
    {
        public int Id { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastUsedAt { get; set; }
        public string Permissions { get; set; } = "[]"; // JSON array
    }
}