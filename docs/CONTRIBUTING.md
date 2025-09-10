# 🤝 Contributing to BTHL CheckGate
# Enterprise Monitoring Platform Contribution Guide

**We welcome** contributions from developers, security researchers, and system administrators who share our vision for enterprise-grade monitoring solutions.

---

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contributing Guidelines](#contributing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing Requirements](#testing-requirements)
- [Documentation Standards](#documentation-standards)
- [Security Guidelines](#security-guidelines)

---

## 📜 Code of Conduct

**We maintain** a welcoming, professional environment for all contributors. **Our community** follows these principles:

- **Be respectful** and inclusive in all interactions
- **Focus on constructive** feedback and collaboration
- **Welcome newcomers** and help them get started
- **Maintain professional** communication standards
- **Report inappropriate behavior** to the maintainers

---

## 🚀 Getting Started

### Prerequisites

**We require** these tools for development:

- **Visual Studio 2022** or **VS Code** with C# extensions
- **.NET 9 SDK** (latest stable)
- **MySQL 8.0** (local instance)
- **Docker Desktop** (for Kubernetes monitoring)
- **Node.js 20+** (for React development)
- **Git** (latest version)

### Repository Setup

```bash
# Fork the repository on GitHub
# Clone your fork
git clone https://github.com/YOUR_USERNAME/BTHL-CheckGate.git
cd BTHL-CheckGate

# Add upstream remote
git remote add upstream https://github.com/davestj/BTHL-CheckGate.git

# Install dependencies
dotnet restore
cd client/admin-dashboard && npm install && cd ../..
```

---

## 🛠️ Development Setup

### 1. Database Setup

```sql
-- Create development database
CREATE DATABASE bthl_checkgate_dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create test user (optional)
CREATE USER 'dev_user'@'localhost' IDENTIFIED BY 'DevPassword123!';
GRANT ALL PRIVILEGES ON bthl_checkgate_dev.* TO 'dev_user'@'localhost';
FLUSH PRIVILEGES;
```

### 2. Configuration

Create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=bthl_checkgate_dev;Uid=root;Pwd=YOUR_PASSWORD;AllowUserVariables=True"
  },
  "Jwt": {
    "SecretKey": "development-secret-key-change-in-production",
    "Issuer": "BTHLCheckGate-Dev",
    "Audience": "BTHLCheckGate-Dev-Users",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

### 3. Run Development Environment

```bash
# Start the application
dotnet run --project src/BTHLCheckGate.Service -- --console --port 9300

# In another terminal, start React development server
cd client/admin-dashboard
npm run dev
```

---

## 📝 Contributing Guidelines

### Types of Contributions

**We accept** these types of contributions:

1. **🐛 Bug Fixes** - Fix identified issues
2. **✨ Features** - Add new functionality
3. **📚 Documentation** - Improve or add documentation
4. **🔒 Security** - Security improvements and fixes
5. **⚡ Performance** - Performance optimizations
6. **🧪 Tests** - Add or improve test coverage
7. **♻️ Refactoring** - Code quality improvements

### Before You Start

**We recommend** discussing significant changes before implementation:

1. **Check existing issues** for similar work
2. **Create an issue** to discuss your proposal
3. **Get approval** from maintainers for major changes
4. **Follow our coding standards** and conventions

---

## 🔄 Pull Request Process

### 1. Branch Naming

**We use** descriptive branch names:

```bash
# Feature branches
git checkout -b feature/add-prometheus-metrics
git checkout -b feature/kubernetes-multi-cluster

# Bug fix branches  
git checkout -b fix/jwt-token-expiration
git checkout -b fix/memory-leak-monitoring-service

# Security branches
git checkout -b security/fix-sql-injection-vulnerability
git checkout -b security/update-dependencies
```

### 2. Commit Messages

**We follow** conventional commit format:

```bash
# Format: type(scope): description
feat(api): add Prometheus metrics endpoint
fix(auth): resolve JWT token expiration issue
docs(readme): update installation instructions
test(monitoring): add unit tests for system metrics
security(auth): fix potential SQL injection in user service
refactor(service): improve monitoring service performance
```

### 3. Pull Request Template

**We provide** a template for all pull requests:

```markdown
## 📋 Description
Brief description of changes and motivation.

## 🔄 Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update
- [ ] Security improvement

## 🧪 Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed
- [ ] Security testing (if applicable)

## 📚 Documentation
- [ ] Documentation updated (if needed)
- [ ] API documentation updated (if needed)
- [ ] README updated (if needed)

## ✅ Checklist
- [ ] My code follows the project's coding standards
- [ ] I have performed a self-review of my code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] My changes generate no new warnings
- [ ] New and existing unit tests pass locally
```

### 4. Review Process

**We maintain** quality through comprehensive reviews:

1. **Automated checks** must pass (build, tests, security scan)
2. **Code review** by at least one maintainer
3. **Security review** for security-related changes
4. **Documentation review** for user-facing changes
5. **Performance testing** for performance-critical changes

---

## 💻 Coding Standards

### C# Coding Standards

**We follow** Microsoft's C# coding conventions with these additions:

#### Naming Conventions
```csharp
// Use PascalCase for public members
public class SystemMonitoringService { }
public interface IMetricsCollector { }
public enum MetricType { CPU, Memory, Disk }

// Use camelCase for private fields with underscore prefix
private readonly ILogger _logger;
private readonly string _connectionString;

// Use descriptive names
public async Task<SystemMetrics> CollectCurrentSystemMetricsAsync()
{
    // Implementation
}
```

#### Documentation Standards
```csharp
/// <summary>
/// We collect comprehensive system metrics including CPU, memory, disk, and network data.
/// Our implementation uses Windows Management Instrumentation (WMI) providers for
/// accurate and efficient data collection suitable for enterprise monitoring.
/// </summary>
/// <param name="includeProcesses">Whether to include detailed process information</param>
/// <returns>Complete system metrics snapshot with timestamp</returns>
/// <exception cref="SystemMonitoringException">Thrown when WMI providers are unavailable</exception>
public async Task<SystemMetrics> CollectSystemMetricsAsync(bool includeProcesses = false)
{
    // Implementation with comprehensive error handling
}
```

#### Error Handling
```csharp
public async Task<Result<SystemMetrics>> GetSystemMetricsAsync()
{
    try
    {
        _logger.LogDebug("Starting system metrics collection");
        
        var metrics = await CollectMetricsFromWmi();
        
        _logger.LogInformation("Successfully collected system metrics for {Hostname}", 
            Environment.MachineName);
            
        return Result.Success(metrics);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogError(ex, "Insufficient privileges for system metrics collection");
        return Result.Failure<SystemMetrics>("Insufficient privileges for monitoring");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error during metrics collection");
        return Result.Failure<SystemMetrics>("System metrics collection failed");
    }
}
```

### React/TypeScript Standards

```typescript
// Use interfaces for type definitions
interface SystemMetrics {
  timestamp: string;
  hostname: string;
  cpu: CpuMetrics;
  memory: MemoryMetrics;
}

// Use functional components with hooks
export const SystemMetricsChart: React.FC<SystemMetricsChartProps> = ({ 
  metrics, 
  refreshInterval = 30000 
}) => {
  const [data, setData] = useState<SystemMetrics[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  
  useEffect(() => {
    // Implementation with cleanup
    const interval = setInterval(fetchMetrics, refreshInterval);
    return () => clearInterval(interval);
  }, [refreshInterval]);
  
  return (
    <div className="metrics-chart">
      {loading ? <LoadingSpinner /> : <Chart data={data} />}
    </div>
  );
};
```

---

## 🧪 Testing Requirements

### Test Coverage Requirements

**We maintain** comprehensive test coverage:

- **Unit Tests**: >80% code coverage
- **Integration Tests**: All API endpoints
- **End-to-End Tests**: Critical user flows
- **Security Tests**: Authentication and authorization
- **Performance Tests**: Load and stress testing

### Unit Testing Standards

```csharp
[Fact]
public async Task CollectSystemMetricsAsync_WhenWmiAvailable_ReturnsValidMetrics()
{
    // Arrange
    var mockWmiService = new Mock<IWmiService>();
    mockWmiService.Setup(x => x.GetCpuMetricsAsync())
               .ReturnsAsync(new CpuMetrics { UtilizationPercent = 25.0 });
    
    var service = new SystemMonitoringService(mockWmiService.Object, _logger);
    
    // Act
    var result = await service.CollectSystemMetricsAsync();
    
    // Assert
    Assert.NotNull(result);
    Assert.True(result.Cpu.UtilizationPercent > 0);
    Assert.Equal(Environment.MachineName, result.Hostname);
}

[Theory]
[InlineData(-1)]
[InlineData(101)]
public void ValidateCpuUtilization_WhenInvalidValue_ThrowsException(double invalidValue)
{
    // Arrange & Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => 
        new CpuMetrics { UtilizationPercent = invalidValue });
}
```

### Integration Testing

```csharp
public class SystemMetricsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    [Fact]
    public async Task GetCurrentMetrics_WithValidToken_ReturnsMetrics()
    {
        // Arrange
        var token = await GetValidJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/v1/systemmetrics/current");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var metrics = JsonSerializer.Deserialize<SystemMetrics>(content);
        
        Assert.NotNull(metrics);
        Assert.NotEmpty(metrics.Hostname);
    }
}
```

---

## 📚 Documentation Standards

### Collaborative Commentary Pattern

**We use** first-person plural language throughout documentation:

```markdown
<!-- Good -->
**We implemented** JWT authentication to ensure secure API access.
**Our approach** uses industry-standard practices for token management.
**We chose** this architecture for scalability and maintainability.

<!-- Avoid -->
The system implements JWT authentication.
This approach uses industry-standard practices.
This architecture was chosen for scalability.
```

### API Documentation

```csharp
/// <summary>
/// We provide comprehensive system metrics collection with real-time monitoring
/// capabilities. Our endpoint returns detailed CPU, memory, disk, and network
/// metrics suitable for enterprise monitoring dashboards and alerting systems.
/// </summary>
[HttpGet("current")]
[ProducesResponseType(typeof(SystemMetrics), 200)]
[ProducesResponseType(typeof(ErrorResponse), 401)]
[ProducesResponseType(typeof(ErrorResponse), 500)]
public async Task<ActionResult<SystemMetrics>> GetCurrentMetrics()
{
    // Implementation
}
```

---

## 🔒 Security Guidelines

### Security Review Process

**We require** security review for:

- Authentication and authorization changes
- Input validation modifications
- Database query updates
- External API integrations
- Configuration changes

### Secure Coding Practices

```csharp
// Always validate input
public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
{
    // Validate input parameters
    if (string.IsNullOrWhiteSpace(request.Username))
        return Result.Failure<User>("Username is required");
    
    if (request.Username.Length > 50)
        return Result.Failure<User>("Username too long");
    
    // Sanitize input
    var sanitizedUsername = request.Username.Trim().ToLowerInvariant();
    
    // Use parameterized queries
    var existingUser = await _context.Users
        .Where(u => u.Username == sanitizedUsername)
        .FirstOrDefaultAsync();
        
    if (existingUser != null)
        return Result.Failure<User>("Username already exists");
    
    // Hash passwords properly
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);
    
    // Implementation continues...
}
```

### Security Testing

```csharp
[Fact]
public async Task AuthenticateAsync_WithSqlInjectionAttempt_ReturnsFailure()
{
    // Arrange
    var maliciousInput = "admin'; DROP TABLE users; --";
    
    // Act
    var result = await _authService.AuthenticateAsync(maliciousInput, "password");
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("Invalid username", result.Error);
}
```

---

## 🏆 Recognition

**We recognize** valuable contributions:

### Contributor Recognition

- **Code Contributors**: Listed in CONTRIBUTORS.md
- **Security Researchers**: Acknowledged in security advisories
- **Documentation Contributors**: Credited in documentation sections
- **Bug Reporters**: Thanked in release notes

### Contribution Levels

| **Level** | **Criteria** | **Recognition** |
|---|---|---|
| **Contributor** | 1+ merged PR | Listed in CONTRIBUTORS.md |
| **Regular Contributor** | 5+ merged PRs | GitHub badge + special thanks |
| **Core Contributor** | 25+ merged PRs + reviews | Maintainer consideration |
| **Security Contributor** | Security vulnerability report/fix | Hall of fame + credit |

---

## 📞 Getting Help

**We provide** multiple support channels:

- **💬 GitHub Discussions**: General questions and ideas
- **🐛 GitHub Issues**: Bug reports and feature requests
- **📧 Email**: For security issues - security@bthlcorp.com
- **📚 Documentation**: Comprehensive guides in `/docs`

---

**We appreciate** your interest in contributing to BTHL CheckGate. **Your contributions** help make enterprise monitoring more accessible and reliable for organizations worldwide.

*Ready to contribute? Start by exploring our [good first issue](https://github.com/davestj/BTHL-CheckGate/labels/good%20first%20issue) label on GitHub.*