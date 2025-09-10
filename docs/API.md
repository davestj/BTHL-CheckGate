# 🌐 BTHL CheckGate API Documentation
# REST API v1 Reference Guide

**Version**: 1.0.0  
**Base URL**: `https://localhost:9300/api/v1`  
**Authentication**: JWT Bearer Token  
**Content-Type**: `application/json`

---

## 📋 Table of Contents

- [Authentication](#authentication)
- [System Metrics API](#system-metrics-api)
- [Kubernetes API](#kubernetes-api)
- [Admin API](#admin-api)
- [Health & Status API](#health--status-api)
- [Error Handling](#error-handling)
- [Rate Limiting](#rate-limiting)
- [Code Examples](#code-examples)

---

## 🔐 Authentication

All API endpoints require authentication using JWT Bearer tokens.

### Get Authentication Token

```http
POST /api/v1/auth/token
Content-Type: application/json

{
  "username": "admin",
  "password": "your_password"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-09-10T18:30:00Z",
  "tokenType": "Bearer"
}
```

### Using Authentication Token

Include the token in all subsequent requests:

```http
GET /api/v1/systemmetrics/current
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📊 System Metrics API

Monitor real-time system performance and resource utilization.

### GET /api/v1/systemmetrics/current

Get current system metrics snapshot.

**Headers:**
- `Authorization: Bearer <token>`

**Response:**
```json
{
  "timestamp": "2025-09-10T15:30:00Z",
  "hostname": "WORKSTATION-01",
  "cpu": {
    "utilizationPercent": 25.4,
    "coreCount": 8,
    "threadCount": 16,
    "frequency": 3200,
    "temperature": 42.5
  },
  "memory": {
    "totalBytes": 17179869184,
    "availableBytes": 8589934592,
    "usedBytes": 8589934592,
    "utilizationPercent": 50.0,
    "pageFileUsage": 4294967296
  },
  "disk": [
    {
      "drive": "C:",
      "totalBytes": 1000000000000,
      "freeBytes": 500000000000,
      "usedBytes": 500000000000,
      "utilizationPercent": 50.0,
      "fileSystem": "NTFS"
    }
  ],
  "network": [
    {
      "interfaceName": "Ethernet",
      "bytesReceived": 1024000000,
      "bytesSent": 512000000,
      "packetsReceived": 1000000,
      "packetsSent": 500000,
      "isActive": true,
      "speed": 1000000000
    }
  ]
}
```

### GET /api/v1/systemmetrics/history

Get historical system metrics with time range filtering.

**Parameters:**
- `startDate` (string, ISO 8601): Start date for metrics
- `endDate` (string, ISO 8601): End date for metrics
- `interval` (string): Aggregation interval (1m, 5m, 15m, 1h, 1d)
- `limit` (integer): Maximum number of results (default: 1000)

**Example:**
```http
GET /api/v1/systemmetrics/history?startDate=2025-09-09T00:00:00Z&endDate=2025-09-10T00:00:00Z&interval=15m
```

**Response:**
```json
{
  "metrics": [
    {
      "timestamp": "2025-09-09T00:00:00Z",
      "cpu": { "utilizationPercent": 15.2 },
      "memory": { "utilizationPercent": 45.1 },
      "disk": [{ "drive": "C:", "utilizationPercent": 48.3 }]
    }
  ],
  "totalRecords": 96,
  "interval": "15m"
}
```

### GET /api/v1/systemmetrics/processes/top

Get top CPU and memory consuming processes.

**Parameters:**
- `count` (integer): Number of processes to return (default: 10)
- `sortBy` (string): Sort criteria (cpu, memory, handles, threads)

**Response:**
```json
{
  "processes": [
    {
      "processId": 1234,
      "processName": "chrome.exe",
      "cpuPercent": 15.4,
      "workingSetBytes": 512000000,
      "handleCount": 1024,
      "threadCount": 32,
      "startTime": "2025-09-10T08:00:00Z"
    }
  ],
  "sortedBy": "cpu",
  "totalProcesses": 147
}
```

---

## ☸️ Kubernetes API

Monitor Kubernetes cluster health and workloads.

### GET /api/v1/kubernetes/cluster/status

Get overall cluster health and status.

**Response:**
```json
{
  "clusterName": "docker-desktop",
  "status": "Healthy",
  "version": "v1.28.2",
  "nodes": {
    "total": 1,
    "ready": 1,
    "notReady": 0
  },
  "pods": {
    "total": 12,
    "running": 10,
    "pending": 1,
    "failed": 1
  },
  "namespaces": [
    "default",
    "kube-system",
    "kube-public",
    "kube-node-lease"
  ],
  "lastUpdate": "2025-09-10T15:30:00Z"
}
```

### GET /api/v1/kubernetes/nodes

Get detailed node information and resource utilization.

**Response:**
```json
{
  "nodes": [
    {
      "name": "docker-desktop",
      "status": "Ready",
      "role": "control-plane",
      "version": "v1.28.2",
      "architecture": "amd64",
      "operatingSystem": "linux",
      "resources": {
        "cpu": {
          "capacity": "8",
          "allocatable": "8",
          "usage": "2.5"
        },
        "memory": {
          "capacity": "16Gi",
          "allocatable": "15Gi",
          "usage": "8Gi"
        },
        "storage": {
          "capacity": "100Gi",
          "allocatable": "95Gi",
          "usage": "45Gi"
        }
      },
      "conditions": [
        {
          "type": "Ready",
          "status": "True",
          "lastUpdate": "2025-09-10T15:30:00Z"
        }
      ]
    }
  ]
}
```

### GET /api/v1/kubernetes/pods

Get pod information across all namespaces.

**Parameters:**
- `namespace` (string): Filter by namespace (optional)
- `status` (string): Filter by status (running, pending, succeeded, failed)

**Response:**
```json
{
  "pods": [
    {
      "name": "coredns-76f75df574-xyz",
      "namespace": "kube-system",
      "status": "Running",
      "node": "docker-desktop",
      "restarts": 0,
      "age": "5d",
      "containers": [
        {
          "name": "coredns",
          "image": "registry.k8s.io/coredns/coredns:v1.10.1",
          "status": "Running",
          "restarts": 0,
          "resources": {
            "requests": {
              "cpu": "100m",
              "memory": "70Mi"
            },
            "limits": {
              "memory": "170Mi"
            }
          }
        }
      ],
      "createdAt": "2025-09-05T10:00:00Z"
    }
  ],
  "totalPods": 12
}
```

---

## 👤 Admin API

Administrative functions for user and system management.

### GET /api/v1/admin/users

Get user list and permissions (Admin only).

**Response:**
```json
{
  "users": [
    {
      "id": "1",
      "username": "admin",
      "email": "admin@company.com",
      "role": "Administrator",
      "lastLogin": "2025-09-10T14:00:00Z",
      "isActive": true,
      "createdAt": "2025-09-01T00:00:00Z"
    }
  ],
  "totalUsers": 1
}
```

### POST /api/v1/admin/users

Create new user account (Admin only).

**Request:**
```json
{
  "username": "newuser",
  "email": "newuser@company.com",
  "password": "SecurePassword123!",
  "role": "User"
}
```

### GET /api/v1/admin/config

Get system configuration settings.

**Response:**
```json
{
  "settings": {
    "monitoringInterval": 30,
    "dataRetentionDays": 90,
    "enableKubernetesMonitoring": true,
    "maxConcurrentUsers": 100,
    "rateLimiting": {
      "requestsPerMinute": 60,
      "burstSize": 10
    },
    "security": {
      "jwtExpirationMinutes": 60,
      "requireHttps": true,
      "passwordComplexity": "Strong"
    }
  }
}
```

---

## 🏥 Health & Status API

Monitor application health and service status.

### GET /api/v1/health

Application health check endpoint (No authentication required).

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2025-09-10T15:30:00Z",
  "version": "1.0.0",
  "services": {
    "database": {
      "status": "Healthy",
      "responseTime": "5ms"
    },
    "kubernetes": {
      "status": "Healthy",
      "responseTime": "15ms"
    },
    "systemMonitoring": {
      "status": "Healthy",
      "responseTime": "2ms"
    }
  },
  "uptime": "5d 12h 30m",
  "environment": "Production"
}
```

### GET /api/v1/status

Detailed system status and metrics.

**Response:**
```json
{
  "application": {
    "name": "BTHL CheckGate",
    "version": "1.0.0",
    "environment": "Production",
    "startTime": "2025-09-05T03:00:00Z"
  },
  "performance": {
    "requestsPerSecond": 45.2,
    "averageResponseTime": "23ms",
    "errorRate": 0.01,
    "activeConnections": 12
  },
  "resources": {
    "memoryUsage": "256MB",
    "cpuUsage": "5%",
    "diskUsage": "45GB"
  }
}
```

---

## ❌ Error Handling

The API uses standard HTTP status codes and provides detailed error information.

### Error Response Format

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input parameters",
    "details": [
      {
        "field": "startDate",
        "message": "Date format must be ISO 8601"
      }
    ],
    "requestId": "req_123456789",
    "timestamp": "2025-09-10T15:30:00Z"
  }
}
```

### Common HTTP Status Codes

| Code | Description | Common Causes |
|------|-------------|---------------|
| 200 | Success | Request completed successfully |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Invalid parameters or malformed request |
| 401 | Unauthorized | Missing or invalid authentication token |
| 403 | Forbidden | Insufficient permissions for operation |
| 404 | Not Found | Requested resource does not exist |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Unexpected server error |
| 503 | Service Unavailable | Service temporarily unavailable |

### Error Codes Reference

| Error Code | Description | Resolution |
|------------|-------------|------------|
| `INVALID_TOKEN` | JWT token is invalid or expired | Refresh authentication token |
| `VALIDATION_ERROR` | Request parameters are invalid | Check parameter format and values |
| `RESOURCE_NOT_FOUND` | Requested resource doesn't exist | Verify resource ID or path |
| `RATE_LIMIT_EXCEEDED` | Too many requests | Wait and retry with exponential backoff |
| `INSUFFICIENT_PERMISSIONS` | User lacks required permissions | Contact administrator for access |

---

## 🚦 Rate Limiting

API requests are rate limited to ensure fair usage and system stability.

### Rate Limits

| Endpoint Type | Requests per Minute | Burst Limit |
|---------------|-------------------|-------------|
| Authentication | 10 | 5 |
| System Metrics | 60 | 10 |
| Kubernetes API | 30 | 8 |
| Admin API | 20 | 5 |
| Health Checks | 120 | 20 |

### Rate Limit Headers

```http
HTTP/1.1 200 OK
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1694358600
```

---

## 💻 Code Examples

### C# Example

```csharp
using System.Text.Json;
using System.Net.Http.Headers;

var client = new HttpClient();
client.BaseAddress = new Uri("https://localhost:9300/api/v1/");

// Authenticate
var authRequest = new { username = "admin", password = "password" };
var authJson = JsonSerializer.Serialize(authRequest);
var authContent = new StringContent(authJson, Encoding.UTF8, "application/json");

var authResponse = await client.PostAsync("auth/token", authContent);
var authResult = await authResponse.Content.ReadAsStringAsync();
var token = JsonDocument.Parse(authResult).RootElement.GetProperty("token").GetString();

// Set authentication header
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

// Get system metrics
var metricsResponse = await client.GetAsync("systemmetrics/current");
var metricsJson = await metricsResponse.Content.ReadAsStringAsync();
Console.WriteLine(metricsJson);
```

### PowerShell Example

```powershell
# Authenticate
$authBody = @{
    username = "admin"
    password = "password"
} | ConvertTo-Json

$authResponse = Invoke-RestMethod -Uri "https://localhost:9300/api/v1/auth/token" `
    -Method POST -Body $authBody -ContentType "application/json" `
    -SkipCertificateCheck

$token = $authResponse.token

# Get system metrics
$headers = @{ Authorization = "Bearer $token" }
$metrics = Invoke-RestMethod -Uri "https://localhost:9300/api/v1/systemmetrics/current" `
    -Method GET -Headers $headers -SkipCertificateCheck

Write-Output $metrics
```

### JavaScript/TypeScript Example

```typescript
interface AuthResponse {
  token: string;
  expiresAt: string;
  tokenType: string;
}

interface SystemMetrics {
  timestamp: string;
  hostname: string;
  cpu: {
    utilizationPercent: number;
    coreCount: number;
  };
  memory: {
    totalBytes: number;
    usedBytes: number;
    utilizationPercent: number;
  };
}

class BTHLCheckGateAPI {
  private baseUrl = 'https://localhost:9300/api/v1';
  private token?: string;

  async authenticate(username: string, password: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/auth/token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });

    const auth: AuthResponse = await response.json();
    this.token = auth.token;
  }

  async getCurrentMetrics(): Promise<SystemMetrics> {
    if (!this.token) throw new Error('Not authenticated');
    
    const response = await fetch(`${this.baseUrl}/systemmetrics/current`, {
      headers: { 'Authorization': `Bearer ${this.token}` }
    });

    return await response.json();
  }
}

// Usage
const api = new BTHLCheckGateAPI();
await api.authenticate('admin', 'password');
const metrics = await api.getCurrentMetrics();
console.log('CPU Usage:', metrics.cpu.utilizationPercent + '%');
```

### Python Example

```python
import requests
import json
from datetime import datetime

class BTHLCheckGateAPI:
    def __init__(self, base_url="https://localhost:9300/api/v1"):
        self.base_url = base_url
        self.token = None
        self.session = requests.Session()
        # Disable SSL verification for development
        self.session.verify = False

    def authenticate(self, username, password):
        auth_data = {"username": username, "password": password}
        response = self.session.post(f"{self.base_url}/auth/token", json=auth_data)
        response.raise_for_status()
        
        auth_result = response.json()
        self.token = auth_result["token"]
        self.session.headers.update({"Authorization": f"Bearer {self.token}"})

    def get_current_metrics(self):
        response = self.session.get(f"{self.base_url}/systemmetrics/current")
        response.raise_for_status()
        return response.json()

    def get_kubernetes_status(self):
        response = self.session.get(f"{self.base_url}/kubernetes/cluster/status")
        response.raise_for_status()
        return response.json()

# Usage
api = BTHLCheckGateAPI()
api.authenticate("admin", "password")

metrics = api.get_current_metrics()
print(f"CPU Usage: {metrics['cpu']['utilizationPercent']}%")
print(f"Memory Usage: {metrics['memory']['utilizationPercent']}%")

k8s_status = api.get_kubernetes_status()
print(f"Cluster Status: {k8s_status['status']}")
```

---

## 🔗 Interactive Documentation

For interactive API testing and detailed schema information, visit:

**Swagger UI**: https://localhost:9300/api/docs

The Swagger interface provides:
- **Live API Testing**: Execute requests directly from the browser
- **Schema Documentation**: Detailed request/response models
- **Authentication Integration**: Built-in JWT token management
- **Code Generation**: Generate client code in multiple languages

---

## 📞 Support

For API-related questions and support:

- **📚 Documentation**: Complete guides available in `/docs`
- **🐛 Issues**: [GitHub Issues](https://github.com/davestj/BTHL-CheckGate/issues)
- **💬 API Discussions**: [GitHub Wiki](https://github.com/davestj/BTHL-CheckGate/wiki)
- **📧 Enterprise Support**: Contact api-support@bthlcorp.com

---

*This API documentation is automatically generated from OpenAPI specifications and kept in sync with the latest codebase changes.*