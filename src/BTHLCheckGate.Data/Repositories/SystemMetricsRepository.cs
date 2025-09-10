/**
 * BTHL CheckGate - System Metrics Repository Implementation
 * File: src/BTHLCheckGate.Data/Repositories/SystemMetricsRepository.cs
 */

using BTHLCheckGate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BTHLCheckGate.Data.Repositories
{
    public class SystemMetricsRepository : ISystemMetricsRepository
    {
        private readonly CheckGateDbContext _context;
        private readonly ILogger<SystemMetricsRepository> _logger;

        public SystemMetricsRepository(CheckGateDbContext context, ILogger<SystemMetricsRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SystemMetrics?> GetLatestMetricsAsync()
        {
            try
            {
                var entity = await _context.SystemMetrics
                    .OrderByDescending(m => m.Timestamp)
                    .FirstOrDefaultAsync();

                if (entity == null) return null;

                return await MapEntityToModel(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest system metrics");
                throw;
            }
        }

        public async Task<PagedResult<SystemMetrics>> GetHistoricalMetricsAsync(DateTime startTime, DateTime endTime, int intervalMinutes, int page, int pageSize)
        {
            try
            {
                var query = _context.SystemMetrics
                    .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
                    .OrderBy(m => m.Timestamp);

                var totalItems = await query.CountAsync();
                var entities = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var models = new List<SystemMetrics>();
                foreach (var entity in entities)
                {
                    models.Add(await MapEntityToModel(entity));
                }

                return new PagedResult<SystemMetrics>
                {
                    Data = models,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving historical system metrics");
                throw;
            }
        }

        public async Task<SystemMetricsSummary> GetMetricsSummaryAsync(DateTime startTime, DateTime endTime)
        {
            try
            {
                var metrics = await _context.SystemMetrics
                    .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
                    .ToListAsync();

                if (!metrics.Any())
                {
                    return new SystemMetricsSummary
                    {
                        GeneratedAt = DateTime.UtcNow,
                        PeriodHours = (int)(endTime - startTime).TotalHours,
                        OverallHealth = SystemHealthStatus.Unknown
                    };
                }

                return new SystemMetricsSummary
                {
                    GeneratedAt = DateTime.UtcNow,
                    PeriodHours = (int)(endTime - startTime).TotalHours,
                    CpuSummary = new MetricSummary
                    {
                        Average = metrics.Average(m => m.CpuOverallUtilization),
                        Minimum = metrics.Min(m => m.CpuOverallUtilization),
                        Maximum = metrics.Max(m => m.CpuOverallUtilization),
                        Current = metrics.OrderByDescending(m => m.Timestamp).First().CpuOverallUtilization,
                        Trend = TrendDirection.Stable
                    },
                    MemorySummary = new MetricSummary
                    {
                        Average = metrics.Average(m => (double)(m.MemoryTotalPhysicalBytes - m.MemoryAvailablePhysicalBytes) / m.MemoryTotalPhysicalBytes * 100),
                        Current = (double)(metrics.OrderByDescending(m => m.Timestamp).First().MemoryTotalPhysicalBytes - metrics.OrderByDescending(m => m.Timestamp).First().MemoryAvailablePhysicalBytes) / metrics.OrderByDescending(m => m.Timestamp).First().MemoryTotalPhysicalBytes * 100,
                        Trend = TrendDirection.Stable
                    },
                    OverallHealth = SystemHealthStatus.Good,
                    ActiveAlerts = 0,
                    UptimePercentage = 99.9
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating system metrics summary");
                throw;
            }
        }

        public async Task<List<SystemAlert>> GetActiveAlertsAsync(AlertSeverity? severity, int limit)
        {
            try
            {
                var query = _context.SystemAlerts
                    .Where(a => a.Status == AlertStatus.Active);

                if (severity.HasValue)
                {
                    query = query.Where(a => a.Severity == severity.Value);
                }

                var entities = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(limit)
                    .ToListAsync();

                return entities.Select(MapAlertEntityToModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active alerts");
                throw;
            }
        }

        public async Task StoreSystemMetricsAsync(SystemMetrics metrics)
        {
            try
            {
                var entity = new SystemMetricsEntity
                {
                    Timestamp = metrics.Timestamp,
                    Hostname = metrics.Hostname,
                    CpuOverallUtilization = metrics.Cpu.OverallUtilization,
                    CpuCoreUtilization = JsonSerializer.Serialize(metrics.Cpu.CoreUtilization),
                    CpuLogicalProcessors = metrics.Cpu.LogicalProcessors,
                    CpuTemperature = metrics.Cpu.Temperature,
                    CpuFrequencyMhz = metrics.Cpu.FrequencyMhz,
                    MemoryTotalPhysicalBytes = metrics.Memory.TotalPhysicalBytes,
                    MemoryAvailablePhysicalBytes = metrics.Memory.AvailablePhysicalBytes,
                    MemoryTotalVirtualBytes = metrics.Memory.TotalVirtualBytes,
                    MemoryAvailableVirtualBytes = metrics.Memory.AvailableVirtualBytes,
                    MemoryPageFileBytes = metrics.Memory.PageFileBytes,
                    ProcessesTotalProcesses = metrics.Processes.TotalProcesses,
                    ProcessesTotalThreads = metrics.Processes.TotalThreads
                };

                _context.SystemMetrics.Add(entity);
                await _context.SaveChangesAsync();

                // Store disk metrics
                foreach (var disk in metrics.Disks)
                {
                    _context.DiskMetrics.Add(new DiskMetricsEntity
                    {
                        SystemMetricsId = entity.Id,
                        DriveLetter = disk.DriveLetter,
                        Label = disk.Label,
                        TotalSizeBytes = disk.TotalSizeBytes,
                        FreeSpaceBytes = disk.FreeSpaceBytes,
                        ReadOperationsPerSecond = disk.ReadOperationsPerSecond,
                        WriteOperationsPerSecond = disk.WriteOperationsPerSecond
                    });
                }

                // Store network metrics
                foreach (var network in metrics.NetworkInterfaces)
                {
                    _context.NetworkMetrics.Add(new NetworkMetricsEntity
                    {
                        SystemMetricsId = entity.Id,
                        InterfaceName = network.InterfaceName,
                        BytesReceivedPerSecond = network.BytesReceivedPerSecond,
                        BytesSentPerSecond = network.BytesSentPerSecond,
                        ErrorsReceived = network.ErrorsReceived,
                        ErrorsSent = network.ErrorsSent
                    });
                }

                // Store top processes
                foreach (var process in metrics.Processes.TopCpuProcesses)
                {
                    _context.ProcessInfo.Add(new ProcessInfoEntity
                    {
                        SystemMetricsId = entity.Id,
                        ProcessId = process.ProcessId,
                        ProcessName = process.ProcessName,
                        ProcessType = "CPU",
                        CpuUtilization = process.CpuUtilization,
                        MemoryUsageBytes = process.MemoryUsageBytes
                    });
                }

                foreach (var process in metrics.Processes.TopMemoryProcesses)
                {
                    _context.ProcessInfo.Add(new ProcessInfoEntity
                    {
                        SystemMetricsId = entity.Id,
                        ProcessId = process.ProcessId,
                        ProcessName = process.ProcessName,
                        ProcessType = "Memory",
                        CpuUtilization = process.CpuUtilization,
                        MemoryUsageBytes = process.MemoryUsageBytes
                    });
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing system metrics");
                throw;
            }
        }

        private async Task<SystemMetrics> MapEntityToModel(SystemMetricsEntity entity)
        {
            var disks = await _context.DiskMetrics
                .Where(d => d.SystemMetricsId == entity.Id)
                .Select(d => new DiskMetrics
                {
                    DriveLetter = d.DriveLetter,
                    Label = d.Label,
                    TotalSizeBytes = d.TotalSizeBytes,
                    FreeSpaceBytes = d.FreeSpaceBytes,
                    ReadOperationsPerSecond = d.ReadOperationsPerSecond,
                    WriteOperationsPerSecond = d.WriteOperationsPerSecond
                })
                .ToListAsync();

            var networks = await _context.NetworkMetrics
                .Where(n => n.SystemMetricsId == entity.Id)
                .Select(n => new NetworkMetrics
                {
                    InterfaceName = n.InterfaceName,
                    BytesReceivedPerSecond = n.BytesReceivedPerSecond,
                    BytesSentPerSecond = n.BytesSentPerSecond,
                    ErrorsReceived = n.ErrorsReceived,
                    ErrorsSent = n.ErrorsSent
                })
                .ToListAsync();

            var topCpuProcesses = await _context.ProcessInfo
                .Where(p => p.SystemMetricsId == entity.Id && p.ProcessType == "CPU")
                .Select(p => new ProcessInfo
                {
                    ProcessId = p.ProcessId,
                    ProcessName = p.ProcessName,
                    CpuUtilization = p.CpuUtilization,
                    MemoryUsageBytes = p.MemoryUsageBytes
                })
                .ToListAsync();

            var topMemoryProcesses = await _context.ProcessInfo
                .Where(p => p.SystemMetricsId == entity.Id && p.ProcessType == "Memory")
                .Select(p => new ProcessInfo
                {
                    ProcessId = p.ProcessId,
                    ProcessName = p.ProcessName,
                    CpuUtilization = p.CpuUtilization,
                    MemoryUsageBytes = p.MemoryUsageBytes
                })
                .ToListAsync();

            return new SystemMetrics
            {
                Timestamp = entity.Timestamp,
                Hostname = entity.Hostname,
                Cpu = new CpuMetrics
                {
                    OverallUtilization = entity.CpuOverallUtilization,
                    CoreUtilization = JsonSerializer.Deserialize<List<double>>(entity.CpuCoreUtilization) ?? new List<double>(),
                    LogicalProcessors = entity.CpuLogicalProcessors,
                    Temperature = entity.CpuTemperature,
                    FrequencyMhz = entity.CpuFrequencyMhz
                },
                Memory = new MemoryMetrics
                {
                    TotalPhysicalBytes = entity.MemoryTotalPhysicalBytes,
                    AvailablePhysicalBytes = entity.MemoryAvailablePhysicalBytes,
                    TotalVirtualBytes = entity.MemoryTotalVirtualBytes,
                    AvailableVirtualBytes = entity.MemoryAvailableVirtualBytes,
                    PageFileBytes = entity.MemoryPageFileBytes
                },
                Disks = disks,
                NetworkInterfaces = networks,
                Processes = new ProcessMetrics
                {
                    TotalProcesses = entity.ProcessesTotalProcesses,
                    TotalThreads = entity.ProcessesTotalThreads,
                    TopCpuProcesses = topCpuProcesses,
                    TopMemoryProcesses = topMemoryProcesses
                }
            };
        }

        private static SystemAlert MapAlertEntityToModel(SystemAlertEntity entity)
        {
            return new SystemAlert
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Severity = entity.Severity,
                Type = entity.Type,
                Title = entity.Title,
                Description = entity.Description,
                Source = entity.Source,
                Hostname = entity.Hostname,
                Status = entity.Status,
                TriggerValue = entity.TriggerValue,
                ThresholdValue = entity.ThresholdValue,
                Unit = entity.Unit,
                Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Metadata) ?? new Dictionary<string, object>(),
                AcknowledgedAt = entity.AcknowledgedAt,
                AcknowledgedBy = entity.AcknowledgedBy,
                ResolvedAt = entity.ResolvedAt,
                ResolvedBy = entity.ResolvedBy,
                ResolutionNotes = entity.ResolutionNotes
            };
        }
    }
}