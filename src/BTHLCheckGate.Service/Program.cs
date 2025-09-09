// src/BTHLCheckGate.Service/Program.cs
/**
 * BTHL CheckGate - Windows Service Host Entry Point
 * File: src/BTHLCheckGate.Service/Program.cs
 * 
 * We are creating the main entry point for our Windows service application.
 * Our design allows this application to run both as a console application during development
 * and as a proper Windows service in production environments.
 * 
 * @author David St John <davestj@gmail.com>
 * @version 1.0.0
 * @since 2025-09-09
 * 
 * CHANGELOG:
 * 2025-09-09 - FEAT: Initial service host implementation with dual-mode support
 */

using BTHLCheckGate.Core.Services;
using BTHLCheckGate.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace BTHLCheckGate.Service
{
    /// <summary>
    /// We define the main program class that orchestrates our service hosting capabilities.
    /// Our implementation supports both console mode for development and Windows service mode for production.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// We implement the main entry point that processes command line arguments and determines execution mode.
        /// Our argument parsing allows for flexible deployment scenarios while maintaining security.
        /// </summary>
        /// <param name="args">Command line arguments passed to our application</param>
        /// <returns>Exit code indicating success or failure of our service execution</returns>
        public static async Task<int> Main(string[] args)
        {
            // We are setting up our command line interface to provide professional-grade argument handling
            var rootCommand = new RootCommand("BTHL CheckGate - Enterprise System Monitoring Service")
            {
                new Option<bool>("--service", "Run as Windows service"),
                new Option<bool>("--console", "Run as console application"),
                new Option<int>("--port", () => 9300, "HTTP/HTTPS port number"),
                new Option<string>("--config", "Configuration file path"),
                new Option<bool>("--install", "Install Windows service"),
                new Option<bool>("--uninstall", "Uninstall Windows service"),
                new Option<LogLevel>("--log-level", () => LogLevel.Information, "Logging level")
            };

            // We configure our command handler to process the parsed arguments appropriately
            rootCommand.SetHandler(async (bool isService, bool isConsole, int port, string configPath, 
                bool install, bool uninstall, LogLevel logLevel) =>
            {
                try
                {
                    // We handle service installation and uninstallation operations first
                    if (install)
                    {
                        await ServiceInstaller.InstallServiceAsync();
                        return;
                    }

                    if (uninstall)
                    {
                        await ServiceInstaller.UninstallServiceAsync();
                        return;
                    }

                    // We determine our execution mode based on arguments or environment
                    bool runAsService = isService || (!isConsole && Environment.UserInteractive == false);

                    // We create our host builder with the appropriate configuration
                    var hostBuilder = CreateHostBuilder(args, port, configPath, logLevel);

                    if (runAsService)
                    {
                        // We configure for Windows service execution
                        hostBuilder.UseWindowsService();
                    }

                    var host = hostBuilder.Build();
                    await host.RunAsync();
                }
                catch (Exception ex)
                {
                    // We log critical startup errors for troubleshooting
                    Console.WriteLine($"Critical error during startup: {ex.Message}");
                    Environment.ExitCode = 1;
                }
            });

            return await rootCommand.InvokeAsync(args);
        }

        /// <summary>
        /// We construct our host builder with all necessary services and configurations.
        /// Our builder pattern ensures proper dependency injection and service registration.
        /// </summary>
        /// <param name="args">Original command line arguments for configuration</param>
        /// <param name="port">Port number for our web server</param>
        /// <param name="configPath">Optional custom configuration file path</param>
        /// <param name="logLevel">Minimum logging level for our application</param>
        /// <returns>Configured host builder ready for service execution</returns>
        private static IHostBuilder CreateHostBuilder(string[] args, int port, string configPath, LogLevel logLevel)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // We register our core monitoring services
                    services.AddSingleton<ISystemMonitoringService, SystemMonitoringService>();
                    services.AddSingleton<IKubernetesMonitoringService, KubernetesMonitoringService>();
                    services.AddSingleton<IMetricsCollectionService, MetricsCollectionService>();
                    
                    // We add our main service worker
                    services.AddHostedService<CheckGateWorkerService>();
                    
                    // We register our web API hosting
                    services.AddHostedService<WebApiHostService>();
                })
                .ConfigureLogging(logging =>
                {
                    // We configure comprehensive logging for enterprise environments
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddEventLog(); // Windows Event Log integration
                    logging.SetMinimumLevel(logLevel);
                });
        }
    }

    /// <summary>
    /// We implement our main worker service that coordinates system monitoring activities.
    /// Our service runs continuously and manages data collection at configured intervals.
    /// </summary>
    public class CheckGateWorkerService : BackgroundService
    {
        private readonly ILogger<CheckGateWorkerService> _logger;
        private readonly ISystemMonitoringService _systemMonitoring;
        private readonly IKubernetesMonitoringService _kubernetesMonitoring;
        private readonly IMetricsCollectionService _metricsCollection;

        /// <summary>
        /// We initialize our worker service with all required monitoring dependencies.
        /// Our constructor uses dependency injection to ensure loose coupling and testability.
        /// </summary>
        public CheckGateWorkerService(
            ILogger<CheckGateWorkerService> logger,
            ISystemMonitoringService systemMonitoring,
            IKubernetesMonitoringService kubernetesMonitoring,
            IMetricsCollectionService metricsCollection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _systemMonitoring = systemMonitoring ?? throw new ArgumentNullException(nameof(systemMonitoring));
            _kubernetesMonitoring = kubernetesMonitoring ?? throw new ArgumentNullException(nameof(kubernetesMonitoring));
            _metricsCollection = metricsCollection ?? throw new ArgumentNullException(nameof(metricsCollection));
        }

        /// <summary>
        /// We execute our main monitoring loop with proper error handling and performance optimization.
        /// Our implementation ensures continuous operation while respecting system resources.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token for graceful shutdown</param>
        /// <returns>Task representing our monitoring execution</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BTHL CheckGate service is starting monitoring operations...");

            // We implement our main monitoring loop with configurable intervals
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // We collect system metrics in parallel for optimal performance
                    var systemMetricsTask = _systemMonitoring.CollectSystemMetricsAsync();
                    var kubernetesMetricsTask = _kubernetesMonitoring.CollectClusterMetricsAsync();

                    await Task.WhenAll(systemMetricsTask, kubernetesMetricsTask);

                    // We store our collected metrics for API consumption
                    await _metricsCollection.StoreMetricsAsync(systemMetricsTask.Result, kubernetesMetricsTask.Result);

                    _logger.LogDebug("Metrics collection cycle completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during metrics collection cycle");
                }

                // We wait for our configured interval before the next collection cycle
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("BTHL CheckGate service is stopping monitoring operations...");
        }
    }
}

// src/BTHLCheckGate.Models/SystemMetrics.cs
/**
 * BTHL CheckGate - System Metrics Data Models
 * File: src/BTHLCheckGate.Models/SystemMetrics.cs
 * 
 * We define our data models for system resource metrics collection and API responses.
 * Our models provide comprehensive system information while maintaining type safety.
 * 
 * @author David St John <davestj@gmail.com>
 * @version 1.0.0
 * @since 2025-09-09
 * 
 * CHANGELOG:
 * 2025-09-09 - FEAT: Initial system metrics models for enterprise monitoring
 */

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTHLCheckGate.Models
{
    /// <summary>
    /// We represent comprehensive system resource metrics in a structured format.
    /// Our model captures all essential system information needed for monitoring dashboards.
    /// </summary>
    public class SystemMetrics
    {
        /// <summary>
        /// We track when these metrics were collected for temporal analysis
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// We provide CPU utilization information across all cores and logical processors
        /// </summary>
        [Required]
        public CpuMetrics Cpu { get; set; } = new();

        /// <summary>
        /// We capture memory usage patterns including virtual and physical memory
        /// </summary>
        [Required]
        public MemoryMetrics Memory { get; set; } = new();

        /// <summary>
        /// We monitor all disk drives and their utilization characteristics
        /// </summary>
        [Required]
        public List<DiskMetrics> Disks { get; set; } = new();

        /// <summary>
        /// We track network interface statistics and throughput information
        /// </summary>
        [Required]
        public List<NetworkMetrics> NetworkInterfaces { get; set; } = new();

        /// <summary>
        /// We capture running process information for system analysis
        /// </summary>
        [Required]
        public ProcessMetrics Processes { get; set; } = new();
    }

    /// <summary>
    /// We model CPU performance characteristics and utilization patterns.
    /// Our design captures both instant and historical CPU usage information.
    /// </summary>
    public class CpuMetrics
    {
        /// <summary>
        /// We report overall CPU utilization as a percentage (0-100)
        /// </summary>
        [Range(0, 100)]
        public double OverallUtilization { get; set; }

        /// <summary>
        /// We provide per-core utilization for detailed performance analysis
        /// </summary>
        public List<double> CoreUtilization { get; set; } = new();

        /// <summary>
        /// We track the number of logical processors available to our system
        /// </summary>
        public int LogicalProcessors { get; set; }

        /// <summary>
        /// We capture CPU temperature when available from hardware sensors
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// We record current CPU frequency for performance monitoring
        /// </summary>
        public double? FrequencyMhz { get; set; }
    }

    /// <summary>
    /// We structure memory utilization data for comprehensive memory analysis.
    /// Our model includes both physical and virtual memory characteristics.
    /// </summary>
    public class MemoryMetrics
    {
        /// <summary>
        /// We report total physical memory installed in the system (bytes)
        /// </summary>
        public long TotalPhysicalBytes { get; set; }

        /// <summary>
        /// We track available physical memory for allocation (bytes)
        /// </summary>
        public long AvailablePhysicalBytes { get; set; }

        /// <summary>
        /// We calculate physical memory utilization percentage
        /// </summary>
        [JsonIgnore]
        public double PhysicalUtilizationPercent => 
            TotalPhysicalBytes > 0 ? ((TotalPhysicalBytes - AvailablePhysicalBytes) / (double)TotalPhysicalBytes) * 100 : 0;

        /// <summary>
        /// We monitor virtual memory limits and usage patterns
        /// </summary>
        public long TotalVirtualBytes { get; set; }

        /// <summary>
        /// We track available virtual memory for process allocation
        /// </summary>
        public long AvailableVirtualBytes { get; set; }

        /// <summary>
        /// We capture page file usage for virtual memory analysis
        /// </summary>
        public long PageFileBytes { get; set; }
    }

    /// <summary>
    /// We model individual disk drive metrics for storage monitoring.
    /// Our structure captures both capacity and performance characteristics.
    /// </summary>
    public class DiskMetrics
    {
        /// <summary>
        /// We identify the drive using its letter designation (C:, D:, etc.)
        /// </summary>
        [Required]
        public string DriveLetter { get; set; } = string.Empty;

        /// <summary>
        /// We provide a human-readable label for the drive
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// We report total storage capacity in bytes
        /// </summary>
        public long TotalSizeBytes { get; set; }

        /// <summary>
        /// We track available free space in bytes
        /// </summary>
        public long FreeSpaceBytes { get; set; }

        /// <summary>
        /// We calculate storage utilization percentage
        /// </summary>
        [JsonIgnore]
        public double UtilizationPercent => 
            TotalSizeBytes > 0 ? ((TotalSizeBytes - FreeSpaceBytes) / (double)TotalSizeBytes) * 100 : 0;

        /// <summary>
        /// We capture disk read operations per second for performance analysis
        /// </summary>
        public double ReadOperationsPerSecond { get; set; }

        /// <summary>
        /// We monitor disk write operations per second for I/O analysis
        /// </summary>
        public double WriteOperationsPerSecond { get; set; }
    }

    /// <summary>
    /// We represent network interface statistics for connectivity monitoring.
    /// Our model captures both throughput and error characteristics.
    /// </summary>
    public class NetworkMetrics
    {
        /// <summary>
        /// We identify the network interface by its system name
        /// </summary>
        [Required]
        public string InterfaceName { get; set; } = string.Empty;

        /// <summary>
        /// We track bytes received per second for bandwidth analysis
        /// </summary>
        public long BytesReceivedPerSecond { get; set; }

        /// <summary>
        /// We monitor bytes sent per second for upload bandwidth tracking
        /// </summary>
        public long BytesSentPerSecond { get; set; }

        /// <summary>
        /// We capture network errors for connectivity quality assessment
        /// </summary>
        public long ErrorsReceived { get; set; }

        /// <summary>
        /// We track transmission errors for network reliability monitoring
        /// </summary>
        public long ErrorsSent { get; set; }
    }

    /// <summary>
    /// We aggregate process information for system resource analysis.
    /// Our model provides insights into system load and process distribution.
    /// </summary>
    public class ProcessMetrics
    {
        /// <summary>
        /// We count total running processes for system load assessment
        /// </summary>
        public int TotalProcesses { get; set; }

        /// <summary>
        /// We track total threads across all processes
        /// </summary>
        public int TotalThreads { get; set; }

        /// <summary>
        /// We identify the top CPU-consuming processes for performance analysis
        /// </summary>
        public List<ProcessInfo> TopCpuProcesses { get; set; } = new();

        /// <summary>
        /// We list the top memory-consuming processes for resource monitoring
        /// </summary>
        public List<ProcessInfo> TopMemoryProcesses { get; set; } = new();
    }

    /// <summary>
    /// We detail individual process characteristics for resource attribution.
    /// Our structure provides essential process identification and resource usage.
    /// </summary>
    public class ProcessInfo
    {
        /// <summary>
        /// We identify the process by its unique system identifier
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// We provide the process name for human identification
        /// </summary>
        [Required]
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// We report CPU utilization percentage for this specific process
        /// </summary>
        [Range(0, 100)]
        public double CpuUtilization { get; set; }

        /// <summary>
        /// We track memory usage in bytes for this process
        /// </summary>
        public long MemoryUsageBytes { get; set; }
    }
}
