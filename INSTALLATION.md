# INSTALLATION.md - BTHL CheckGate Complete Setup Guide

## Overview

**We provide** comprehensive installation instructions for BTHL CheckGate, an enterprise-grade Windows system monitoring platform. **Our guide** ensures successful deployment across development, testing, and production environments with professional-grade security and performance configurations.

## System Requirements

### Minimum Requirements

**We require** the following minimum system specifications for reliable operation:

- **Operating System**: Windows 11 Pro (recommended) or Windows Server 2019+
- **Memory**: 8 GB RAM minimum, 16 GB recommended
- **Storage**: 50 GB available disk space (database and logs will grow over time)
- **Network**: Gigabit Ethernet adapter for optimal performance
- **CPU**: 4-core processor (Intel i5 equivalent or better)

### Software Prerequisites

**We depend** on these software components that must be installed before BTHL CheckGate:

- **.NET 8 Runtime**: Latest LTS version for optimal performance and security
- **MySQL 8.0**: Database server for metrics storage and configuration
- **Docker Desktop**: Required for Kubernetes monitoring capabilities
- **Visual Studio 2022**: For development and debugging (development environments only)
- **Git**: For source code management and updates
- **Node.js 20.x**: For building the frontend admin dashboard

### Network Requirements

**We configure** the following network settings for proper operation:

- **Port 9300**: HTTPS web interface and API endpoints
- **Port 3306**: MySQL database connection (localhost only)
- **Port 443**: Outbound HTTPS for updates and external integrations
- **Kubernetes API**: Access to Docker Desktop Kubernetes cluster

## Pre-Installation Checklist

### Administrative Access

**We require** administrative privileges for proper installation:

1. **Verify** you are logged in with a local Administrator account
2. **Confirm** User Account Control (UAC) is configured appropriately
3. **Ensure** Windows Defender or antivirus software allows our installation
4. **Check** that Windows Firewall is configured to allow new program installations

### Environment Preparation

**We recommend** completing these preparation steps:

1. **Update** Windows to the latest version with all security patches
2. **Install** all Windows Updates and restart the system
3. **Configure** automatic Windows Updates for security maintenance
4. **Verify** system clock is synchronized with a reliable time source
5. **Ensure** adequate disk space is available on the system drive

## Installation Methods

**We provide** multiple installation approaches to suit different deployment scenarios:

### Method 1: Automated Installation (Recommended)

**We recommend** this approach for production deployments and demonstrations:

1. **Download** the latest release package from our GitHub repository
2. **Extract** the installation files to a temporary directory
3. **Run** the automated installation script with administrative privileges

```powershell
# We execute the automated installation with default settings
.\deployment\scripts\Install-BTHLCheckGate.ps1

# We can customize the installation with specific parameters
.\deployment\scripts\Install-BTHLCheckGate.ps1 -InstallPath "C:\BTHL\CheckGate" -Port 9300
```

### Method 2: Manual Installation

**We support** manual installation for customized deployments:

#### Step 1: Database Setup

```sql
-- We create the MySQL database and user
CREATE DATABASE bthl_checkgate CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'bthl_checkgate'@'localhost' IDENTIFIED BY 'YourSecurePassword123!';
GRANT ALL PRIVILEGES ON bthl_checkgate.* TO 'bthl_checkgate'@'localhost';
FLUSH PRIVILEGES;

-- We apply the database schema
SOURCE database/schema/01-initial-schema.sql;
```

#### Step 2: Application Deployment

```powershell
# We create the installation directory
New-Item -Path "C:\Program Files\BTHL\CheckGate" -ItemType Directory -Force

# We copy application files
Copy-Item -Path "publish\*" -Destination "C:\Program Files\BTHL\CheckGate\" -Recurse

# We create the configuration file
# (Edit appsettings.Production.json with your database connection string)
```

#### Step 3: Service Installation

```powershell
# We register the Windows service
New-Service -Name "BTHLCheckGate" -BinaryPathName "C:\Program Files\BTHL\CheckGate\BTHLCheckGate.Service.exe --service" -DisplayName "BTHL CheckGate Monitoring Service" -StartupType Automatic

# We start the service
Start-Service -Name "BTHLCheckGate"
```

### Method 3: Development Environment Setup

**We provide** special instructions for development environments:

#### Prerequisites for Development

```powershell
# We install development tools using Chocolatey (optional but recommended)
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# We install required development software
choco install dotnet-8.0-runtime dotnet-8.0-sdk nodejs mysql git vscode -y
```

#### Development Database Setup

```bash
# We start MySQL using Docker (alternative to local installation)
docker run --name mysql-checkgate -e MYSQL_ROOT_PASSWORD=DevPassword123! -e MYSQL_DATABASE=bthl_checkgate_dev -p 3306:3306 -d mysql:8.0

# We wait for MySQL to be ready
docker exec mysql-checkgate mysqladmin --user=root --password=DevPassword123! --host="127.0.0.1" ping --silent
```

#### Building and Running for Development

```bash
# We clone the repository and navigate to the project directory
git clone https://github.com/bthlcorp/bthl-checkgate.git
cd bthl-checkgate

# We restore .NET dependencies
dotnet restore BTHLCheckGate.sln

# We build the frontend
cd client/admin-dashboard
npm install
npm run build
cd ../..

# We build the solution
dotnet build BTHLCheckGate.sln --configuration Debug

# We run the application in console mode for development
dotnet run --project src/BTHLCheckGate.Service/BTHLCheckGate.Service.csproj -- --console --port 9300
```

## Configuration

### Database Configuration

**We configure** the database connection in the `appsettings.Production.json` file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=bthl_checkgate;Uid=bthl_checkgate;Pwd=YourSecurePassword123!;AllowUserVariables=True"
  }
}
```

### Security Configuration

**We implement** comprehensive security settings:

```json
{
  "Jwt": {
    "Issuer": "BTHLCheckGate",
    "Audience": "BTHLCheckGate-Users",
    "SecretKey": "YourJWTSecretKey-MustBe256BitsOrLonger-ChangeThisInProduction"
  },
  "CheckGate": {
    "RequireHttps": true,
    "AllowedOrigins": ["https://localhost:9300"],
    "RateLimitPerHour": 1000
  }
}
```

### Monitoring Configuration

**We customize** monitoring behavior through configuration:

```json
{
  "CheckGate": {
    "MonitoringInterval": 30,
    "DataRetentionDays": 90,
    "EnableKubernetesMonitoring": true,
    "KubernetesClusterName": "docker-desktop",
    "AlertThresholds": {
      "CpuWarning": 80.0,
      "CpuCritical": 95.0,
      "MemoryWarning": 85.0,
      "MemoryCritical": 95.0,
      "DiskWarning": 80.0,
      "DiskCritical": 95.0
    }
  }
}
```

## Post-Installation Verification

### Service Status Verification

**We verify** the service is running correctly:

```powershell
# We check service status
Get-Service -Name "BTHLCheckGate"

# We verify the service is listening on the configured port
netstat -an | findstr :9300

# We check the Windows Event Log for any errors
Get-EventLog -LogName Application -Source "BTHLCheckGate" -Newest 10
```

### Web Interface Testing

**We access** the web interface to verify installation:

1. **Open** a web browser and navigate to `https://localhost:9300`
2. **Accept** the self-signed certificate warning (development only)
3. **Login** using Windows authentication (Administrator account)
4. **Verify** the dashboard displays system metrics
5. **Check** that all metric cards show current data

### API Endpoint Testing

**We test** the REST API functionality:

```powershell
# We use the included test client to verify API functionality
.\client\test-client\BTHLCheckGate.TestClient.exe --base-url https://localhost:9300 --interactive

# We can also test individual endpoints with PowerShell
$headers = @{
    'Authorization' = 'Bearer YOUR_JWT_TOKEN_HERE'
    'Content-Type' = 'application/json'
}

Invoke-RestMethod -Uri "https://localhost:9300/api/v1/systemmetrics/current" -Headers $headers -Method GET
```

### Database Verification

**We confirm** database connectivity and data collection:

```sql
-- We check that metrics are being collected
SELECT COUNT(*) as metric_count, MAX(timestamp) as latest_metric 
FROM system_metrics 
WHERE timestamp > DATE_SUB(NOW(), INTERVAL 1 HOUR);

-- We verify table structure is correct
SHOW TABLES;
DESCRIBE system_metrics;
```

## Troubleshooting

### Common Installation Issues

#### Issue: Service Fails to Start

**We diagnose** service startup problems:

```powershell
# We check the Windows Event Log for detailed error messages
Get-EventLog -LogName Application -Source "BTHLCheckGate" -EntryType Error -Newest 5

# We run the service in console mode for debugging
cd "C:\Program Files\BTHL\CheckGate"
.\BTHLCheckGate.Service.exe --console --verbose
```

**Solution**: Most service startup issues are related to:
- Incorrect database connection string
- Missing .NET 8 runtime
- Insufficient permissions
- Port conflicts

#### Issue: Database Connection Failures

**We resolve** database connectivity problems:

```powershell
# We test MySQL connectivity
mysql -h localhost -u bthl_checkgate -p bthl_checkgate

# We verify MySQL service is running
Get-Service -Name "MySQL80"

# We check MySQL configuration
mysql -u root -p -e "SHOW VARIABLES LIKE 'bind_address';"
```

**Solution**: Common database issues include:
- MySQL service not running
- Incorrect credentials in connection string
- Firewall blocking local connections
- MySQL bind_address not set to accept local connections

#### Issue: Web Interface Not Accessible

**We troubleshoot** web access problems:

```powershell
# We verify the service is listening on the correct port
netstat -an | findstr :9300

# We check Windows Firewall rules
Get-NetFirewallRule -DisplayName "*CheckGate*"

# We test HTTPS certificate
[Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
Invoke-WebRequest -Uri "https://localhost:9300" -Method HEAD
```

**Solution**: Web interface issues typically involve:
- Windows Firewall blocking the port
- Service not listening on configured port
- SSL certificate problems in development
- Browser security settings

### Performance Optimization

#### Database Performance

**We optimize** database performance for large-scale monitoring:

```sql
-- We optimize MySQL configuration for time-series data
SET GLOBAL innodb_buffer_pool_size = 1073741824; -- 1GB
SET GLOBAL innodb_log_file_size = 268435456; -- 256MB
SET GLOBAL innodb_flush_log_at_trx_commit = 2;

-- We create additional indexes for common queries
CREATE INDEX idx_system_metrics_timestamp_hostname ON system_metrics(timestamp, hostname);
CREATE INDEX idx_disk_metrics_timestamp ON disk_metrics(system_metrics_id, drive_letter);
```

#### Memory Usage Optimization

**We configure** .NET runtime for optimal memory usage:

```json
{
  "CheckGate": {
    "GCSettings": {
      "ServerGC": true,
      "ConcurrentGC": true,
      "RetainVM": false
    },
    "MetricsCollectionBatchSize": 100,
    "DatabaseConnectionPoolSize": 10
  }
}
```

### Monitoring and Maintenance

#### Log File Management

**We configure** automatic log rotation and cleanup:

```powershell
# We create a scheduled task for log cleanup
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\Program Files\BTHL\CheckGate\Scripts\Cleanup-Logs.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At "02:00AM"
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount
Register-ScheduledTask -TaskName "BTHLCheckGate-LogCleanup" -Action $action -Trigger $trigger -Principal $principal
```

#### Health Monitoring

**We implement** proactive health monitoring:

```powershell
# We create a monitoring script for system health
$healthCheck = @"
# Health check script for BTHL CheckGate
`$service = Get-Service -Name "BTHLCheckGate" -ErrorAction SilentlyContinue
if (`$service.Status -ne "Running") {
    Write-EventLog -LogName Application -Source "BTHLCheckGate-Monitor" -EventId 1001 -EntryType Warning -Message "Service is not running"
    Start-Service -Name "BTHLCheckGate"
}

# Check API endpoint health
try {
    `$response = Invoke-WebRequest -Uri "https://localhost:9300/health" -UseBasicParsing -TimeoutSec 10
    if (`$response.StatusCode -ne 200) {
        Write-EventLog -LogName Application -Source "BTHLCheckGate-Monitor" -EventId 1002 -EntryType Warning -Message "Health check failed"
    }
} catch {
    Write-EventLog -LogName Application -Source "BTHLCheckGate-Monitor" -EventId 1003 -EntryType Error -Message "Health check exception: `$(`$_.Exception.Message)"
}
"@

$healthCheck | Out-File -FilePath "C:\Program Files\BTHL\CheckGate\Scripts\Health-Check.ps1" -Encoding UTF8
```

## Security Considerations

### Production Security Checklist

**We implement** these security measures for production deployments:

- [ ] **Change default passwords**: Update all default credentials
- [ ] **Enable HTTPS**: Configure proper SSL certificates (not self-signed)
- [ ] **Network security**: Implement proper firewall rules
- [ ] **Database security**: Use encrypted connections and strong passwords
- [ ] **Access control**: Configure proper Windows authentication
- [ ] **Audit logging**: Enable comprehensive audit trails
- [ ] **Updates**: Establish automatic security update procedures

### SSL Certificate Configuration

**We configure** proper SSL certificates for production:

```powershell
# We import a proper SSL certificate for production use
Import-PfxCertificate -FilePath "C:\Certificates\checkgate.bthlcorp.com.pfx" -CertStoreLocation Cert:\LocalMachine\My -Password (ConvertTo-SecureString "CertificatePassword" -AsPlainText -Force)

# We bind the certificate to our port
netsh http add sslcert ipport=0.0.0.0:9300 certhash=CERTIFICATE_THUMBPRINT_HERE appid="{GUID_FOR_APPLICATION}"
```

## Backup and Recovery

### Database Backup

**We implement** automated database backups:

```bash
#!/bin/bash
# We create automated MySQL backups
BACKUP_DIR="/var/backups/bthl-checkgate"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/bthl_checkgate_backup_$DATE.sql"

mkdir -p $BACKUP_DIR
mysqldump -u bthl_checkgate -p bthl_checkgate > $BACKUP_FILE
gzip $BACKUP_FILE

# We retain only the last 30 days of backups
find $BACKUP_DIR -name "*.sql.gz" -mtime +30 -delete
```

### Configuration Backup

**We backup** critical configuration files:

```powershell
# We create a configuration backup script
$backupPath = "C:\Backups\BTHLCheckGate\$(Get-Date -Format 'yyyyMMdd')"
New-Item -Path $backupPath -ItemType Directory -Force

# We copy configuration files
Copy-Item "C:\Program Files\BTHL\CheckGate\appsettings.Production.json" $backupPath
Copy-Item "C:\Program Files\BTHL\CheckGate\*.config" $backupPath -ErrorAction SilentlyContinue

# We backup the database schema
mysqldump -u bthl_checkgate -p --no-data bthl_checkgate > "$backupPath\schema_backup.sql"
```

## Support and Maintenance

### Getting Help

**We provide** multiple support channels:

- **Documentation**: Complete guides in the `docs/` directory
- **GitHub Issues**: Report bugs and request features
- **Community Forums**: Connect with other users and developers
- **Professional Support**: Enterprise support options available

### Regular Maintenance Tasks

**We recommend** these maintenance procedures:

1. **Weekly**: Review system logs and performance metrics
2. **Monthly**: Update to the latest stable release
3. **Quarterly**: Review and update security configurations
4. **Annually**: Perform comprehensive security audit

This installation guide provides everything needed to successfully deploy BTHL CheckGate in any environment, from development workstations to enterprise production systems. **Our comprehensive approach** ensures reliable operation and professional-grade monitoring capabilities.
