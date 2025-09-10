/**
 * BTHL CheckGate - Entity Framework Database Context
 * File: src/BTHLCheckGate.Data/CheckGateDbContext.cs
 * 
 * We implement our Entity Framework database context for data persistence.
 * Our design provides optimized data access with proper indexing and relationships.
 * 
 * @author David St John <davestj@gmail.com>
 * @version 1.0.0
 * @since 2025-09-09
 * 
 * CHANGELOG:
 * 2025-09-09 - FEAT: Initial database context with entity configurations
 */

using BTHLCheckGate.Models;
using Microsoft.EntityFrameworkCore;

namespace BTHLCheckGate.Data
{
    /// <summary>
    /// We provide our Entity Framework database context for comprehensive monitoring data storage.
    /// Our implementation optimizes for time-series data patterns while maintaining relational integrity.
    /// </summary>
    public class CheckGateDbContext : DbContext
    {
        /// <summary>
        /// We initialize our database context with the provided configuration options.
        /// Our constructor ensures proper dependency injection integration.
        /// </summary>
        /// <param name="options">Entity Framework configuration options</param>
        public CheckGateDbContext(DbContextOptions<CheckGateDbContext> options) : base(options)
        {
        }

        // System Metrics Tables
        
        /// <summary>
        /// We store comprehensive system metrics for historical analysis and trending.
        /// Our design optimizes for time-series queries with proper indexing.
        /// </summary>
        public DbSet<SystemMetricsEntity> SystemMetrics { get; set; } = null!;

        /// <summary>
        /// We track disk metrics separately for detailed storage monitoring.
        /// Our structure enables per-drive analysis and alerting.
        /// </summary>
        public DbSet<DiskMetricsEntity> DiskMetrics { get; set; } = null!;

        /// <summary>
        /// We store network interface metrics for bandwidth and connectivity analysis.
        /// Our design supports multiple network adapters per system.
        /// </summary>
        public DbSet<NetworkMetricsEntity> NetworkMetrics { get; set; } = null!;

        /// <summary>
        /// We maintain top process information for resource attribution and analysis.
        /// Our structure tracks both CPU and memory consuming processes.
        /// </summary>
        public DbSet<ProcessInfoEntity> ProcessInfo { get; set; } = null!;

        // Kubernetes Metrics Tables

        /// <summary>
        /// We store Kubernetes cluster metrics for container orchestration monitoring.
        /// Our design supports multi-cluster environments with proper data isolation.
        /// </summary>
        public DbSet<KubernetesClusterMetricsEntity> KubernetesClusterMetrics { get; set; } = null!;

        /// <summary>
        /// We track individual node information for infrastructure capacity planning.
        /// Our structure provides detailed node health and resource utilization data.
        /// </summary>
        public DbSet<KubernetesNodeEntity> KubernetesNodes { get; set; } = null!;

        /// <summary>
        /// We monitor pod lifecycle and resource consumption for workload analysis.
        /// Our design enables namespace-based filtering and resource tracking.
        /// </summary>
        public DbSet<KubernetesPodEntity> KubernetesPods { get; set; } = null!;

        /// <summary>
        /// We store namespace resource allocation for governance and quota management.
        /// Our structure supports resource limit monitoring and enforcement.
        /// </summary>
        public DbSet<KubernetesNamespaceEntity> KubernetesNamespaces { get; set; } = null!;

        /// <summary>
        /// We track cluster events for troubleshooting and operational insights.
        /// Our design provides event correlation and pattern analysis capabilities.
        /// </summary>
        public DbSet<KubernetesEventEntity> KubernetesEvents { get; set; } = null!;

        // Alert and Notification Tables

        /// <summary>
        /// We store system alerts for comprehensive monitoring and notification management.
        /// Our design supports alert lifecycle tracking and escalation workflows.
        /// </summary>
        public DbSet<SystemAlertEntity> SystemAlerts { get; set; } = null!;

        // Security and Authentication Tables

        /// <summary>
        /// We maintain API token information for secure authentication and authorization.
        /// Our structure provides token lifecycle management and access tracking.
        /// </summary>
        public DbSet<ApiTokenEntity> ApiTokens { get; set; } = null!;

        /// <summary>
        /// We configure our entity relationships and database constraints.
        /// Our implementation optimizes for query performance and data integrity.
        /// </summary>
        /// <param name="modelBuilder">Entity Framework model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // We configure system metrics entity
            modelBuilder.Entity<SystemMetricsEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Hostname).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                
                // We create indexes for optimal query performance
                entity.HasIndex(e => new { e.Timestamp, e.Hostname }).HasDatabaseName("IX_SystemMetrics_Timestamp_Hostname");
                entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_SystemMetrics_Timestamp");
                entity.HasIndex(e => e.Hostname).HasDatabaseName("IX_SystemMetrics_Hostname");
            });

            // We configure disk metrics with foreign key relationship
            modelBuilder.Entity<DiskMetricsEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DriveLetter).HasMaxLength(10).IsRequired();
                entity.Property(e => e.Label).HasMaxLength(255);
                
                // We establish relationship with system metrics
                entity.HasOne<SystemMetricsEntity>()
                      .WithMany()
                      .HasForeignKey(e => e.SystemMetricsId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(e => e.SystemMetricsId).HasDatabaseName("IX_DiskMetrics_SystemMetricsId");
            });

            // We configure network metrics with foreign key relationship
            modelBuilder.Entity<NetworkMetricsEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InterfaceName).HasMaxLength(255).IsRequired();
                
                // We establish relationship with system metrics
                entity.HasOne<SystemMetricsEntity>()
                      .WithMany()
                      .HasForeignKey(e => e.SystemMetricsId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(e => e.SystemMetricsId).HasDatabaseName("IX_NetworkMetrics_SystemMetricsId");
            });

            // We configure process information with foreign key relationship
            modelBuilder.Entity<ProcessInfoEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProcessName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.ProcessType).HasMaxLength(50).IsRequired();
                
                // We establish relationship with system metrics
                entity.HasOne<SystemMetricsEntity>()
                      .WithMany()
                      .HasForeignKey(e => e.SystemMetricsId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(e => e.SystemMetricsId).HasDatabaseName("IX_ProcessInfo_SystemMetricsId");
            });

            // We configure Kubernetes cluster metrics
            modelBuilder.Entity<KubernetesClusterMetricsEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClusterName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Version).HasMaxLength(50);
                entity.Property(e => e.Timestamp).IsRequired();
                
                // We create indexes for optimal cluster query performance
                entity.HasIndex(e => new { e.Timestamp, e.ClusterName }).HasDatabaseName("IX_KubernetesClusterMetrics_Timestamp_ClusterName");
                entity.HasIndex(e => e.ClusterName).HasDatabaseName("IX_KubernetesClusterMetrics_ClusterName");
            });

            // We configure Kubernetes nodes with foreign key relationship
            modelBuilder.Entity<KubernetesNodeEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                entity.Property(e => e.KubernetesVersion).HasMaxLength(50);
                entity.Property(e => e.OperatingSystem).HasMaxLength(100);
                
                // We establish relationship with cluster metrics
                entity.HasOne<KubernetesClusterMetricsEntity>()
                      .WithMany()
                      .HasForeignKey(e => e.ClusterMetricsId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(e => e.ClusterMetricsId).HasDatabaseName("IX_KubernetesNodes_ClusterMetricsId");
                entity.HasIndex(e => e.Name).HasDatabaseName("IX_KubernetesNodes_Name");
            });

            // We configure Kubernetes pods with foreign key relationship
            modelBuilder.Entity<KubernetesPodEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Namespace).HasMaxLength(255).IsRequired();
                entity.Property(e => e.NodeName).HasMaxLength(255);
                
                // We establish relationship with cluster metrics
                entity.HasOne<KubernetesClusterMetricsEntity>()
                      .WithMany()
                      .HasForeignKey(e => e.ClusterMetricsId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(e => e.ClusterMetricsId).HasDatabaseName("IX_KubernetesPods_ClusterMetricsId");
                entity.HasIndex(e => new { e.Namespace, e.Name }).HasDatabaseName("IX_KubernetesPods_Namespace_Name");
            });

            // We configure Kubernetes namespaces with foreign key relationship
            modelBuilder.Entity<KubernetesNamespaceEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                
                // We establish relationship with cluster metrics
                entity.HasOne<KubernetesClusterMetricsEntity>()
                      .WithMany()
                      .HasForeignKey(e => e.ClusterMetricsId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(e => e.ClusterMetricsId).HasDatabaseName("IX_KubernetesNamespaces_ClusterMetricsId");
                entity.HasIndex(e => e.Name).HasDatabaseName("IX_KubernetesNamespaces_Name");
            });

            // We configure Kubernetes events with foreign key relationship
            modelBuilder.Entity<KubernetesEventEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Reason).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Message).HasColumnType("TEXT");
                entity.Property(e => e.ObjectName).HasMaxLength(255);
                entity.Property(e => e.ObjectKind).HasMaxLength(50);
                entity.Property(e => e.Namespace).HasMaxLength(255);
                
                // We establish relationship with cluster metrics
                entity.HasOne<KubernetesClusterMetricsEntity>()
                      .WithMany()
                      .HasForeignKey(e => e.ClusterMetricsId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(e => e.ClusterMetricsId).HasDatabaseName("IX_KubernetesEvents_ClusterMetricsId");
                entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_KubernetesEvents_Timestamp");
            });

            // We configure system alerts
            modelBuilder.Entity<SystemAlertEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(50);
                entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Description).HasColumnType("TEXT").IsRequired();
                entity.Property(e => e.Source).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Hostname).HasMaxLength(255);
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.Property(e => e.AcknowledgedBy).HasMaxLength(255);
                entity.Property(e => e.ResolvedBy).HasMaxLength(255);
                entity.Property(e => e.ResolutionNotes).HasColumnType("TEXT");
                
                // We create indexes for alert management queries
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_SystemAlerts_Status");
                entity.HasIndex(e => e.Severity).HasDatabaseName("IX_SystemAlerts_Severity");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_SystemAlerts_CreatedAt");
                entity.HasIndex(e => new { e.Status, e.Severity }).HasDatabaseName("IX_SystemAlerts_Status_Severity");
            });

            // We configure API tokens
            modelBuilder.Entity<ApiTokenEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokenHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(255).IsRequired();
                
                // We create indexes for token lookup and management
                entity.HasIndex(e => e.TokenHash).IsUnique().HasDatabaseName("IX_ApiTokens_TokenHash");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_ApiTokens_IsActive");
                entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("IX_ApiTokens_ExpiresAt");
            });
        }

        /// <summary>
        /// We configure database connection optimizations for production performance.
        /// Our settings ensure optimal connection pooling and query execution.
        /// </summary>
        /// <param name="optionsBuilder">Entity Framework options builder</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // We provide default configuration for development environments
                optionsBuilder.UseMySql(
                    "Server=localhost;Database=bthl_checkgate_dev;Uid=dev_user;Pwd=CHANGEME123!;",
                    ServerVersion.AutoDetect("Server=localhost;Database=bthl_checkgate_dev;Uid=dev_user;Pwd=CHANGEME123!;"),
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)
                );
            }
        }
    }
}