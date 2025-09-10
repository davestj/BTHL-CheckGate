# 🔧 BTHL CheckGate - Troubleshooting Guide
# Common Issues and Solutions

**We provide** comprehensive troubleshooting information to help you quickly resolve common issues and get BTHL CheckGate running smoothly in your environment.

---

## 📋 Table of Contents

- [Quick Diagnostics](#quick-diagnostics)
- [Installation Issues](#installation-issues)
- [Database Problems](#database-problems)
- [Authentication Issues](#authentication-issues)
- [API Connection Problems](#api-connection-problems)
- [Kubernetes Monitoring Issues](#kubernetes-monitoring-issues)
- [Performance Problems](#performance-problems)
- [Security & SSL Issues](#security--ssl-issues)
- [Logging & Debugging](#logging--debugging)
- [Getting Additional Help](#getting-additional-help)

---

## 🔍 Quick Diagnostics

**We recommend** running these quick checks before diving into specific troubleshooting:

### System Health Check

```powershell
# Check if the service is running
Get-Service -Name "BTHLCheckGate" -ErrorAction SilentlyContinue

# Test the health endpoint
Invoke-WebRequest -Uri "https://localhost:9300/health" -UseBasicParsing -SkipCertificateCheck

# Check database connectivity
Test-NetConnection -ComputerName "localhost" -Port 3306

# Verify .NET installation
dotnet --version
```

### Log File Locations

| **Component** | **Log Location** | **Purpose** |
|---|---|---|
| **Application** | `logs/bthl-checkgate-{date}.log` | General application logs |
| **Database** | MySQL error log | Database connection issues |
| **Windows Service** | Windows Event Log | Service startup/shutdown |
| **IIS** (if used) | IIS logs | Web server issues |

---

## 🚀 Installation Issues

### Issue: "Administrator privileges required"

**Symptoms**: Installation scripts fail with access denied errors

**Solution**:
```powershell
# Run PowerShell as Administrator
# Right-click PowerShell -> "Run as Administrator"

# Verify admin privileges
[Security.Principal.WindowsIdentity]::GetCurrent().Groups -contains 'S-1-5-32-544'

# If false, you need to run as Administrator
```

### Issue: ".NET 9 not found"

**Symptoms**: `dotnet --version` shows older version or command not found

**Solution**:
```powershell
# Download and install .NET 9
winget install Microsoft.DotNet.SDK.9

# Or download manually from:
# https://dotnet.microsoft.com/download/dotnet/9.0

# Restart PowerShell and verify
dotnet --version
```

### Issue: "MySQL connection failed during setup"

**Symptoms**: Database creation scripts fail with connection errors

**Solution**:
```powershell
# Check MySQL service status
Get-Service -Name "MySQL80" | Select-Object Status, StartType

# If stopped, start the service
Start-Service -Name "MySQL80"

# Test connection manually
mysql -u root -p -e "SELECT VERSION();"

# If password is wrong, reset it:
# Stop MySQL service
# Start MySQL with --skip-grant-tables
# Reset root password
```

### Issue: "Docker Desktop not available"

**Symptoms**: Kubernetes monitoring features don't work

**Solution**:
```powershell
# Check Docker Desktop status
docker version

# If not running, start Docker Desktop
# Enable Kubernetes in Docker Desktop settings
# Settings -> Kubernetes -> Enable Kubernetes
```

---

## 🗄️ Database Problems

### Issue: "Connection string invalid"

**Symptoms**: Application fails to start with database connection errors

**Solution**:
```json
// Check appsettings.json connection string format
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=bthl_checkgate;Uid=root;Pwd=YOUR_PASSWORD;AllowUserVariables=True;SSL Mode=None"
  }
}
```

**Common Connection String Issues**:
- Missing `AllowUserVariables=True` for MySQL
- Incorrect password or special characters not escaped
- Wrong database name (case sensitive on Linux)
- SSL mode conflicts

### Issue: "Database does not exist"

**Symptoms**: Application starts but fails when accessing data

**Solution**:
```sql
-- Manually create the database
CREATE DATABASE bthl_checkgate CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Run the schema script
USE bthl_checkgate;
SOURCE database/schema/01-initial-schema.sql;

-- Verify tables were created
SHOW TABLES;
```

### Issue: "Migration failed"

**Symptoms**: Entity Framework migration errors during startup

**Solution**:
```powershell
# Run migrations manually
dotnet ef database update --project src/BTHLCheckGate.Data --startup-project src/BTHLCheckGate.Service

# If migrations are missing, create them
dotnet ef migrations add InitialCreate --project src/BTHLCheckGate.Data --startup-project src/BTHLCheckGate.Service

# Reset database if needed
dotnet ef database drop --project src/BTHLCheckGate.Data --startup-project src/BTHLCheckGate.Service
dotnet ef database update --project src/BTHLCheckGate.Data --startup-project src/BTHLCheckGate.Service
```

### Issue: "MySQL performance issues"

**Symptoms**: Slow query performance, high CPU usage

**Solution**:
```sql
-- Check slow query log
SET GLOBAL slow_query_log = 'ON';
SET GLOBAL long_query_time = 1;

-- Analyze query performance
SHOW PROCESSLIST;
EXPLAIN SELECT * FROM system_metrics WHERE timestamp > NOW() - INTERVAL 1 HOUR;

-- Optimize indexes
SHOW INDEX FROM system_metrics;
ANALYZE TABLE system_metrics;

-- Consider partitioning for large datasets
ALTER TABLE system_metrics PARTITION BY RANGE(UNIX_TIMESTAMP(timestamp));
```

---

## 🔐 Authentication Issues

### Issue: "JWT token expired"

**Symptoms**: API calls return 401 Unauthorized after working initially

**Solution**:
```csharp
// Check token expiration in appsettings.json
{
  "Jwt": {
    "ExpirationMinutes": 60,  // Increase if needed
    "SecretKey": "your-secret-key"
  }
}
```

**Token Management**:
```javascript
// Implement token refresh in your client
class ApiClient {
    async refreshTokenIfNeeded() {
        const token = localStorage.getItem('jwt_token');
        if (this.isTokenExpired(token)) {
            await this.authenticate();
        }
    }
    
    isTokenExpired(token) {
        if (!token) return true;
        const payload = JSON.parse(atob(token.split('.')[1]));
        return Date.now() >= payload.exp * 1000;
    }
}
```

### Issue: "Windows Authentication not working"

**Symptoms**: Admin dashboard shows login form instead of automatic authentication

**Solution**:
```csharp
// Check Windows Authentication configuration in Startup.cs
services.AddAuthentication(IISDefaults.AuthenticationScheme)
    .AddIISIntegration();

// Ensure Windows Authentication is enabled in IIS
// Or use Kestrel with Windows Auth
```

### Issue: "Invalid credentials"

**Symptoms**: Login always fails even with correct password

**Solution**:
```powershell
# Check if default admin user exists
$connectionString = "Server=localhost;Database=bthl_checkgate;Uid=root;Pwd=YOUR_PASSWORD;"

# Connect and check users table
mysql -u root -p bthl_checkgate -e "SELECT id, username, email FROM users;"

# If no admin user, create one
mysql -u root -p bthl_checkgate -e "
INSERT INTO users (username, email, password_hash, role, is_active, created_at) 
VALUES ('admin', 'admin@company.com', '\$2a\$12\$hashed_password', 'Administrator', 1, NOW());"
```

---

## 🌐 API Connection Problems

### Issue: "API endpoints not responding"

**Symptoms**: All API calls fail with connection timeout

**Diagnostic Steps**:
```powershell
# Check if application is listening on port 9300
netstat -an | findstr :9300

# Test basic connectivity
Test-NetConnection -ComputerName "localhost" -Port 9300

# Check firewall rules
Get-NetFirewallRule -DisplayName "*BTHL*" | Format-Table DisplayName, Enabled, Direction

# Test with curl
curl -k https://localhost:9300/health
```

### Issue: "SSL certificate errors"

**Symptoms**: HTTPS requests fail with certificate validation errors

**Solution**:
```powershell
# For development, trust the development certificate
dotnet dev-certs https --trust

# Or create a new development certificate
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# For production, install a proper certificate
# Use IIS Manager or certlm.msc to install certificates
```

### Issue: "CORS errors in browser"

**Symptoms**: Browser console shows CORS policy errors

**Solution**:
```csharp
// Check CORS configuration in Startup.cs
services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:3000", "https://localhost:3001")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

app.UseCors();  // Must be before UseAuthentication
```

### Issue: "Rate limiting blocking requests"

**Symptoms**: API returns 429 Too Many Requests

**Solution**:
```csharp
// Check rate limiting configuration
services.AddRateLimiter(options =>
{
    options.AddTokenBucketLimiter("api", limiterOptions =>
    {
        limiterOptions.TokenLimit = 100;        // Increase if needed
        limiterOptions.TokensPerPeriod = 60;   // Increase refresh rate
        limiterOptions.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
    });
});
```

---

## ☸️ Kubernetes Monitoring Issues

### Issue: "Kubernetes cluster not detected"

**Symptoms**: Kubernetes endpoints return "cluster not available"

**Diagnostic Steps**:
```powershell
# Check if Docker Desktop Kubernetes is enabled
kubectl cluster-info

# Check if context is set correctly
kubectl config current-context

# Test basic connectivity
kubectl get nodes
kubectl get pods --all-namespaces
```

### Issue: "Kubernetes permissions denied"

**Symptoms**: Application can't access Kubernetes API

**Solution**:
```yaml
# Create a service account with proper permissions
apiVersion: v1
kind: ServiceAccount
metadata:
  name: bthl-checkgate-monitor
  namespace: default
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  name: bthl-checkgate-monitor
rules:
- apiGroups: [""]
  resources: ["nodes", "pods", "namespaces"]
  verbs: ["get", "list", "watch"]
- apiGroups: ["metrics.k8s.io"]
  resources: ["nodes", "pods"]
  verbs: ["get", "list"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: bthl-checkgate-monitor
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: bthl-checkgate-monitor
subjects:
- kind: ServiceAccount
  name: bthl-checkgate-monitor
  namespace: default
```

### Issue: "Kubernetes metrics not updating"

**Symptoms**: Kubernetes dashboard shows stale data

**Solution**:
```csharp
// Check the monitoring service configuration
public class KubernetesMonitoringService
{
    private readonly Timer _timer;
    
    public KubernetesMonitoringService()
    {
        // Reduce polling interval if needed
        _timer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }
}
```

---

## ⚡ Performance Problems

### Issue: "High memory usage"

**Symptoms**: Application uses excessive RAM, memory leaks

**Diagnostic Steps**:
```powershell
# Monitor memory usage
Get-Counter -Counter "\Process(BTHLCheckGate.Service)\Private Bytes" -SampleInterval 5 -MaxSamples 10

# Check for memory leaks in Task Manager or Performance Monitor
# Look for continuously increasing memory usage
```

**Solutions**:
```csharp
// Implement proper disposal in monitoring services
public class SystemMonitoringService : IDisposable
{
    private readonly Timer _timer;
    
    public void Dispose()
    {
        _timer?.Dispose();
        // Dispose other resources
    }
}

// Configure garbage collection
// In Program.cs or appsettings.json
services.Configure<GCSettings>(options =>
{
    options.ServerGarbageCollection = true;
});
```

### Issue: "Slow API response times"

**Symptoms**: API calls take longer than 100ms consistently

**Solution**:
```csharp
// Add response caching
[HttpGet("current")]
[ResponseCache(Duration = 30)] // Cache for 30 seconds
public async Task<ActionResult<SystemMetrics>> GetCurrentMetrics()

// Implement memory caching for expensive operations
private readonly IMemoryCache _cache;

public async Task<SystemMetrics> GetCachedMetricsAsync()
{
    const string cacheKey = "current_metrics";
    
    if (!_cache.TryGetValue(cacheKey, out SystemMetrics metrics))
    {
        metrics = await CollectSystemMetricsAsync();
        _cache.Set(cacheKey, metrics, TimeSpan.FromSeconds(30));
    }
    
    return metrics;
}
```

### Issue: "Database query performance"

**Symptoms**: Database queries taking >100ms

**Solution**:
```sql
-- Add indexes for common queries
CREATE INDEX idx_system_metrics_timestamp ON system_metrics(timestamp);
CREATE INDEX idx_system_metrics_hostname ON system_metrics(hostname);
CREATE INDEX idx_system_metrics_composite ON system_metrics(hostname, timestamp);

-- Analyze slow queries
SHOW FULL PROCESSLIST;
SELECT * FROM information_schema.PROCESSLIST WHERE TIME > 5;

-- Optimize queries
EXPLAIN SELECT * FROM system_metrics 
WHERE hostname = 'server1' AND timestamp > DATE_SUB(NOW(), INTERVAL 1 HOUR);
```

---

## 🔒 Security & SSL Issues

### Issue: "SSL handshake failures"

**Symptoms**: HTTPS connections fail with SSL/TLS errors

**Solution**:
```powershell
# Check SSL certificate validity
certlm.msc  # Open certificate manager
# Navigate to Personal -> Certificates
# Verify certificate is valid and trusted

# Test SSL connection
openssl s_client -connect localhost:9300 -servername localhost

# For development, trust the certificate
dotnet dev-certs https --trust
```

### Issue: "Security headers missing"

**Symptoms**: Security scanners report missing security headers

**Solution**:
```csharp
// Add security headers middleware
public class SecurityHeadersMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        
        var csp = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';";
        context.Response.Headers.Add("Content-Security-Policy", csp);
        
        await next(context);
    }
}
```

---

## 📊 Logging & Debugging

### Enable Debug Logging

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "BTHLCheckGate": "Trace",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Common Log Patterns

**Successful Operation**:
```
[15:30:00 INF] Successfully collected system metrics for WORKSTATION-01
[15:30:00 DBG] CPU: 25.4%, Memory: 8.2GB/16GB, Disk C: 45%
```

**Database Issues**:
```
[15:30:05 ERR] Failed to connect to database: MySql.Data.MySqlClient.MySqlException: Unable to connect to any of the specified MySQL hosts
[15:30:05 ERR] Connection string: Server=localhost;Database=bthl_checkgate;...
```

**Authentication Problems**:
```
[15:30:10 WRN] JWT token validation failed: Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException: Lifetime validation failed
[15:30:10 INF] Request failed authentication: Bearer token expired
```

### Performance Monitoring

```csharp
// Add performance logging
public class PerformanceLoggingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        
        await next(context);
        
        stopwatch.Stop();
        
        if (stopwatch.ElapsedMilliseconds > 100)
        {
            _logger.LogWarning("Slow request: {Method} {Path} took {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

---

## 🆘 Getting Additional Help

### Information to Collect

**When reporting issues, please include**:

1. **Environment Information**:
   ```powershell
   # System info
   systeminfo | findstr /C:"OS Name" /C:"OS Version"
   dotnet --info
   docker version
   mysql --version
   
   # Application logs (last 50 lines)
   Get-Content logs/bthl-checkgate-*.log | Select-Object -Last 50
   ```

2. **Configuration Files** (with sensitive data removed):
   - `appsettings.json`
   - Connection strings
   - Environment variables

3. **Error Messages**:
   - Complete error messages and stack traces
   - Browser console errors (for UI issues)
   - Windows Event Log entries

### Support Channels

| **Issue Type** | **Channel** | **Response Time** |
|---|---|---|
| **Bug Reports** | GitHub Issues | 1-3 business days |
| **Security Issues** | security@bthlcorp.com | 24 hours |
| **General Questions** | GitHub Discussions | 1-5 business days |
| **Enterprise Support** | enterprise@bthlcorp.com | 4 hours |

### Self-Help Resources

- **📚 Documentation**: `/docs` directory
- **🐛 Known Issues**: [GitHub Issues](https://github.com/davestj/BTHL-CheckGate/issues)
- **💬 Community**: [GitHub Discussions](https://github.com/davestj/BTHL-CheckGate/discussions)
- **📖 API Reference**: `https://localhost:9300/api/docs`

---

**We continuously update** this troubleshooting guide based on user feedback and common issues. **If you encounter** a problem not covered here, please let us know so we can help others avoid the same issue.

*Having trouble? Don't hesitate to reach out - we're here to help you get BTHL CheckGate running smoothly in your environment.*