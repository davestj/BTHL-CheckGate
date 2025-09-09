-- database/schema/01-initial-schema.sql
/**
 * BTHL CheckGate - MySQL Database Schema
 * File: database/schema/01-initial-schema.sql
 * 
 * We are designing our database schema to efficiently store system monitoring data
 * while maintaining performance and data integrity. Our approach uses time-series
 * optimizations and proper indexing for enterprise-scale monitoring requirements.
 * 
 * @author David St John <davestj@gmail.com>
 * @version 1.0.0
 * @since 2025-09-09
 * 
 * CHANGELOG:
 * 2025-09-09 - FEAT: Initial database schema for enterprise monitoring platform
 */

-- We create our dedicated database for the CheckGate monitoring system
CREATE DATABASE IF NOT EXISTS bthl_checkgate
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE bthl_checkgate;

-- We establish our system_metrics table to store comprehensive resource data
-- Our design optimizes for fast inserts and time-based queries
CREATE TABLE system_metrics (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    timestamp DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    hostname VARCHAR(255) NOT NULL,
    
    -- We store CPU metrics in structured columns for query performance
    cpu_overall_utilization DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    cpu_core_count INT NOT NULL DEFAULT 0,
    cpu_logical_processors INT NOT NULL DEFAULT 0,
    cpu_temperature DECIMAL(6,2) NULL,
    cpu_frequency_mhz DECIMAL(10,2) NULL,
    
    -- We capture memory information with precision for capacity planning
    memory_total_physical_bytes BIGINT NOT NULL DEFAULT 0,
    memory_available_physical_bytes BIGINT NOT NULL DEFAULT 0,
    memory_total_virtual_bytes BIGINT NOT NULL DEFAULT 0,
    memory_available_virtual_bytes BIGINT NOT NULL DEFAULT 0,
    memory_page_file_bytes BIGINT NOT NULL DEFAULT 0,
    
    -- We track process statistics for system load analysis
    process_total_count INT NOT NULL DEFAULT 0,
    process_total_threads INT NOT NULL DEFAULT 0,
    
    -- We add metadata for troubleshooting and audit purposes
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- We create indexes for optimal query performance
    INDEX idx_timestamp (timestamp),
    INDEX idx_hostname_timestamp (hostname, timestamp),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB 
  PARTITION BY RANGE (UNIX_TIMESTAMP(timestamp)) (
    -- We partition by month for efficient data management and archival
    PARTITION p_202509 VALUES LESS THAN (UNIX_TIMESTAMP('2025-10-01')),
    PARTITION p_202510 VALUES LESS THAN (UNIX_TIMESTAMP('2025-11-01')),
    PARTITION p_202511 VALUES LESS THAN (UNIX_TIMESTAMP('2025-12-01')),
    PARTITION p_202512 VALUES LESS THAN (UNIX_TIMESTAMP('2026-01-01')),
    PARTITION p_future VALUES LESS THAN MAXVALUE
);

-- We create our disk_metrics table for storage monitoring with normalized design
CREATE TABLE disk_metrics (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    system_metrics_id BIGINT NOT NULL,
    drive_letter VARCHAR(10) NOT NULL,
    drive_label VARCHAR(255) NULL,
    total_size_bytes BIGINT NOT NULL DEFAULT 0,
    free_space_bytes BIGINT NOT NULL DEFAULT 0,
    read_operations_per_second DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    write_operations_per_second DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- We establish foreign key relationships for data integrity
    FOREIGN KEY (system_metrics_id) REFERENCES system_metrics(id) ON DELETE CASCADE,
    
    -- We optimize for queries by drive and time relationships
    INDEX idx_system_metrics_id (system_metrics_id),
    INDEX idx_drive_letter (drive_letter)
) ENGINE=InnoDB;

-- We structure our network_metrics table for interface monitoring
CREATE TABLE network_metrics (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    system_metrics_id BIGINT NOT NULL,
    interface_name VARCHAR(255) NOT NULL,
    bytes_received_per_second BIGINT NOT NULL DEFAULT 0,
    bytes_sent_per_second BIGINT NOT NULL DEFAULT 0,
    errors_received BIGINT NOT NULL DEFAULT 0,
    errors_sent BIGINT NOT NULL DEFAULT 0,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- We maintain referential integrity with cascading deletes
    FOREIGN KEY (system_metrics_id) REFERENCES system_metrics(id) ON DELETE CASCADE,
    
    -- We index for efficient interface-based queries
    INDEX idx_system_metrics_id (system_metrics_id),
    INDEX idx_interface_name (interface_name)
) ENGINE=InnoDB;

-- We design our process_metrics table for detailed process tracking
CREATE TABLE process_metrics (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    system_metrics_id BIGINT NOT NULL,
    process_id INT NOT NULL,
    process_name VARCHAR(255) NOT NULL,
    cpu_utilization DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    memory_usage_bytes BIGINT NOT NULL DEFAULT 0,
    metric_type ENUM('top_cpu', 'top_memory') NOT NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- We establish proper relationships for process tracking
    FOREIGN KEY (system_metrics_id) REFERENCES system_metrics(id) ON DELETE CASCADE,
    
    -- We optimize for process analysis queries
    INDEX idx_system_metrics_id (system_metrics_id),
    INDEX idx_metric_type (metric_type),
    INDEX idx_process_name (process_name)
) ENGINE=InnoDB;

-- We create our kubernetes_metrics table for cluster monitoring
CREATE TABLE kubernetes_metrics (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    timestamp DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    cluster_name VARCHAR(255) NOT NULL DEFAULT 'default',
    
    -- We track cluster-wide resource information
    nodes_total INT NOT NULL DEFAULT 0,
    nodes_ready INT NOT NULL DEFAULT 0,
    pods_total INT NOT NULL DEFAULT 0,
    pods_running INT NOT NULL DEFAULT 0,
    pods_pending INT NOT NULL DEFAULT 0,
    pods_failed INT NOT NULL DEFAULT 0,
    
    -- We capture resource utilization across the cluster
    cpu_requests DECIMAL(10,3) NOT NULL DEFAULT 0.000,
    cpu_limits DECIMAL(10,3) NOT NULL DEFAULT 0.000,
    memory_requests_bytes BIGINT NOT NULL DEFAULT 0,
    memory_limits_bytes BIGINT NOT NULL DEFAULT 0,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- We index for time-series queries and cluster analysis
    INDEX idx_timestamp (timestamp),
    INDEX idx_cluster_timestamp (cluster_name, timestamp),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB;

-- We establish our api_tokens table for secure API authentication
CREATE TABLE api_tokens (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    token_name VARCHAR(100) NOT NULL,
    created_by VARCHAR(255) NOT NULL,
    expires_at DATETIME NULL,
    last_used_at DATETIME NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    
    -- We track API usage for security and monitoring
    usage_count BIGINT NOT NULL DEFAULT 0,
    rate_limit_per_hour INT NOT NULL DEFAULT 1000,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- We optimize for token validation and management queries
    INDEX idx_token_hash (token_hash),
    INDEX idx_created_by (created_by),
    INDEX idx_expires_at (expires_at),
    INDEX idx_is_active (is_active)
) ENGINE=InnoDB;

-- We create our audit_log table for security and compliance tracking
CREATE TABLE audit_log (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    timestamp DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    user_identity VARCHAR(255) NOT NULL,
    action VARCHAR(100) NOT NULL,
    resource VARCHAR(255) NOT NULL,
    details JSON NULL,
    ip_address VARCHAR(45) NOT NULL,
    user_agent TEXT NULL,
    success BOOLEAN NOT NULL DEFAULT TRUE,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- We index for security analysis and compliance reporting
    INDEX idx_timestamp (timestamp),
    INDEX idx_user_identity (user_identity),
    INDEX idx_action (action),
    INDEX idx_ip_address (ip_address),
    INDEX idx_success (success)
) ENGINE=InnoDB;

-- We establish our configuration table for runtime settings
CREATE TABLE configuration (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    config_key VARCHAR(255) NOT NULL UNIQUE,
    config_value TEXT NOT NULL,
    config_type ENUM('string', 'number', 'boolean', 'json') NOT NULL DEFAULT 'string',
    description TEXT NULL,
    is_encrypted BOOLEAN NOT NULL DEFAULT FALSE,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- We optimize for configuration retrieval
    INDEX idx_config_key (config_key)
) ENGINE=InnoDB;

-- We insert our initial configuration values for system operation
INSERT INTO configuration (config_key, config_value, config_type, description) VALUES
('monitoring.collection_interval_seconds', '30', 'number', 'Interval between metric collection cycles'),
('monitoring.retention_days', '90', 'number', 'Number of days to retain historical metrics'),
('api.rate_limit_per_hour', '1000', 'number', 'Default API rate limit per hour'),
('web.session_timeout_minutes', '30', 'number', 'Web session timeout in minutes'),
('kubernetes.cluster_name', 'docker-desktop', 'string', 'Default Kubernetes cluster name'),
('security.require_https', 'true', 'boolean', 'Require HTTPS for web access'),
('security.token_expiry_days', '30', 'number', 'Default API token expiry in days');

-- We create our data maintenance procedures for automated cleanup
DELIMITER //

/**
 * We implement our cleanup procedure to maintain optimal database performance.
 * Our approach removes old metrics while preserving recent data for analysis.
 */
CREATE PROCEDURE CleanupOldMetrics()
BEGIN
    DECLARE retention_days INT DEFAULT 90;
    DECLARE cleanup_date DATETIME;
    
    -- We retrieve our configured retention period
    SELECT CAST(config_value AS UNSIGNED) INTO retention_days 
    FROM configuration 
    WHERE config_key = 'monitoring.retention_days';
    
    SET cleanup_date = DATE_SUB(NOW(), INTERVAL retention_days DAY);
    
    -- We remove old system metrics and related data
    DELETE FROM system_metrics WHERE timestamp < cleanup_date;
    
    -- We clean up old Kubernetes metrics
    DELETE FROM kubernetes_metrics WHERE timestamp < cleanup_date;
    
    -- We remove old audit log entries (keeping 1 year for compliance)
    DELETE FROM audit_log WHERE timestamp < DATE_SUB(NOW(), INTERVAL 365 DAY);
    
    -- We log our cleanup operation
    INSERT INTO audit_log (user_identity, action, resource, details) 
    VALUES ('SYSTEM', 'CLEANUP', 'DATABASE', JSON_OBJECT('cleanup_date', cleanup_date));
END //

DELIMITER ;

-- We establish our database user for application access with minimal privileges
CREATE USER IF NOT EXISTS 'bthl_checkgate'@'localhost' IDENTIFIED BY 'CheckGate2025!';

-- We grant appropriate permissions for our application user
GRANT SELECT, INSERT, UPDATE, DELETE ON bthl_checkgate.* TO 'bthl_checkgate'@'localhost';
GRANT EXECUTE ON PROCEDURE bthl_checkgate.CleanupOldMetrics TO 'bthl_checkgate'@'localhost';

-- We flush privileges to ensure changes take effect
FLUSH PRIVILEGES;
