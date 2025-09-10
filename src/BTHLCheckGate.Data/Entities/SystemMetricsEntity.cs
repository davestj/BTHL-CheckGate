/**
 * BTHL CheckGate - System Metrics Database Entity
 * File: src/BTHLCheckGate.Data/Entities/SystemMetricsEntity.cs
 */

using System.ComponentModel.DataAnnotations;

namespace BTHLCheckGate.Data
{
    public class SystemMetricsEntity
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Hostname { get; set; } = string.Empty;
        
        // CPU Metrics
        public double CpuOverallUtilization { get; set; }
        public string CpuCoreUtilization { get; set; } = "[]"; // JSON array
        public int CpuLogicalProcessors { get; set; }
        public double? CpuTemperature { get; set; }
        public double? CpuFrequencyMhz { get; set; }
        
        // Memory Metrics
        public long MemoryTotalPhysicalBytes { get; set; }
        public long MemoryAvailablePhysicalBytes { get; set; }
        public long MemoryTotalVirtualBytes { get; set; }
        public long MemoryAvailableVirtualBytes { get; set; }
        public long MemoryPageFileBytes { get; set; }
        
        // Process Metrics
        public int ProcessesTotalProcesses { get; set; }
        public int ProcessesTotalThreads { get; set; }
    }

    public class DiskMetricsEntity
    {
        public int Id { get; set; }
        public int SystemMetricsId { get; set; }
        public string DriveLetter { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public long TotalSizeBytes { get; set; }
        public long FreeSpaceBytes { get; set; }
        public double ReadOperationsPerSecond { get; set; }
        public double WriteOperationsPerSecond { get; set; }
    }

    public class NetworkMetricsEntity
    {
        public int Id { get; set; }
        public int SystemMetricsId { get; set; }
        public string InterfaceName { get; set; } = string.Empty;
        public long BytesReceivedPerSecond { get; set; }
        public long BytesSentPerSecond { get; set; }
        public long ErrorsReceived { get; set; }
        public long ErrorsSent { get; set; }
    }

    public class ProcessInfoEntity
    {
        public int Id { get; set; }
        public int SystemMetricsId { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string ProcessType { get; set; } = string.Empty; // "CPU" or "Memory"
        public double CpuUtilization { get; set; }
        public long MemoryUsageBytes { get; set; }
    }
}