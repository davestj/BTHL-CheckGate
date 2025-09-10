/**
 * BTHL CheckGate - Kubernetes Metrics Repository Interface
 * File: src/BTHLCheckGate.Data/Repositories/IKubernetesMetricsRepository.cs
 */

using BTHLCheckGate.Models;

namespace BTHLCheckGate.Data.Repositories
{
    public interface IKubernetesMetricsRepository
    {
        Task<KubernetesClusterStatus?> GetClusterStatusAsync(string? clusterName = null);
        Task<PagedResult<KubernetesPod>> GetPodsAsync(string? @namespace, string? status, int page, int pageSize);
        Task StoreClusterMetricsAsync(KubernetesClusterMetrics metrics);
    }
}