// src/BTHLCheckGate.WebApi/Controllers/SystemMetricsController.cs
/**
 * BTHL CheckGate - System Metrics API Controller
 * File: src/BTHLCheckGate.WebApi/Controllers/SystemMetricsController.cs
 * 
 * We are implementing our REST API endpoints for system metrics retrieval.
 * Our controller design follows RESTful principles while providing comprehensive
 * monitoring data access with proper security and performance optimizations.
 * 
 * @author David St John <davestj@gmail.com>
 * @version 1.0.0
 * @since 2025-09-09
 * 
 * CHANGELOG:
 * 2025-09-09 - FEAT: Initial system metrics API with enterprise security features
 */

using BTHLCheckGate.Models;
using BTHLCheckGate.Data.Repositories;
using BTHLCheckGate.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace BTHLCheckGate.WebApi.Controllers
{
    /// <summary>
    /// We provide comprehensive system metrics through our REST API endpoints.
    /// Our controller implements proper HTTP semantics and enterprise security patterns.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EnableRateLimiting("DefaultRateLimitPolicy")]
    [Produces("application/json")]
    public class SystemMetricsController : ControllerBase
    {
        private readonly ISystemMetricsRepository _metricsRepository;
        private readonly ILogger<SystemMetricsController> _logger;

        /// <summary>
        /// We initialize our controller with dependency injection for loose coupling.
        /// Our constructor ensures all required services are available for operation.
        /// </summary>
        public SystemMetricsController(
            ISystemMetricsRepository metricsRepository,
            ILogger<SystemMetricsController> logger)
        {
            _metricsRepository = metricsRepository ?? throw new ArgumentNullException(nameof(metricsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// We retrieve the most recent system metrics for real-time monitoring displays.
        /// Our endpoint provides current system state with optimized query performance.
        /// </summary>
        /// <returns>Latest system metrics data</returns>
        [HttpGet("current")]
        [ProducesResponseType(typeof(SystemMetrics), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SystemMetrics>> GetCurrentMetrics()
        {
            try
            {
                _logger.LogDebug("Retrieving current system metrics");

                var metrics = await _metricsRepository.GetLatestMetricsAsync();
                
                if (metrics == null)
                {
                    _logger.LogWarning("No system metrics found in database");
                    return NotFound("No system metrics available");
                }

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current system metrics");
                return StatusCode(500, "Internal server error occurred");
            }
        }

        /// <summary>
        /// We provide historical metrics data for trend analysis and capacity planning.
        /// Our endpoint supports flexible time range queries with pagination support.
        /// </summary>
        /// <param name="startTime">Beginning of the time range for metrics retrieval</param>
        /// <param name="endTime">End of the time range for metrics retrieval</param>
        /// <param name="intervalMinutes">Aggregation interval in minutes (default: 5)</param>
        /// <param name="page">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Number of records per page (default: 100, max: 1000)</param>
        /// <returns>Historical metrics data with pagination information</returns>
        [HttpGet("historical")]
        [ProducesResponseType(typeof(PagedResult<SystemMetrics>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<SystemMetrics>>> GetHistoricalMetrics(
            [FromQuery, Required] DateTime startTime,
            [FromQuery, Required] DateTime endTime,
            [FromQuery, Range(1, 1440)] int intervalMinutes = 5,
            [FromQuery, Range(1, int.MaxValue)] int page = 1,
            [FromQuery, Range(1, 1000)] int pageSize = 100)
        {
            try
            {
                // We validate the time range to prevent excessive data requests
                if (endTime <= startTime)
                {
                    return BadRequest("End time must be after start time");
                }

                var timeSpan = endTime - startTime;
                if (timeSpan.TotalDays > 30)
                {
                    return BadRequest("Time range cannot exceed 30 days");
                }

                _logger.LogDebug("Retrieving historical metrics from {StartTime} to {EndTime}", 
                    startTime, endTime);

                var result = await _metricsRepository.GetHistoricalMetricsAsync(
                    startTime, endTime, intervalMinutes, page, pageSize);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid parameters for historical metrics request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving historical system metrics");
                return StatusCode(500, "Internal server error occurred");
            }
        }

        /// <summary>
        /// We provide aggregated metrics summaries for dashboard overview displays.
        /// Our endpoint delivers key performance indicators and system health status.
        /// </summary>
        /// <param name="hours">Number of hours to aggregate (default: 24, max: 168)</param>
        /// <returns>Aggregated system metrics summary</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(SystemMetricsSummary), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SystemMetricsSummary>> GetMetricsSummary(
            [FromQuery, Range(1, 168)] int hours = 24)
        {
            try
            {
                _logger.LogDebug("Retrieving metrics summary for last {Hours} hours", hours);

                var endTime = DateTime.UtcNow;
                var startTime = endTime.AddHours(-hours);

                var summary = await _metricsRepository.GetMetricsSummaryAsync(startTime, endTime);

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving metrics summary");
                return StatusCode(500, "Internal server error occurred");
            }
        }

        /// <summary>
        /// We deliver real-time alerts based on configured thresholds and monitoring rules.
        /// Our endpoint provides immediate notification of system issues requiring attention.
        /// </summary>
        /// <param name="severity">Minimum alert severity level (optional)</param>
        /// <param name="limit">Maximum number of alerts to return (default: 50)</param>
        /// <returns>Active system alerts</returns>
        [HttpGet("alerts")]
        [ProducesResponseType(typeof(List<SystemAlert>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<SystemAlert>>> GetActiveAlerts(
            [FromQuery] AlertSeverity? severity = null,
            [FromQuery, Range(1, 100)] int limit = 50)
        {
            try
            {
                _logger.LogDebug("Retrieving active alerts with severity filter: {Severity}", severity);

                var alerts = await _metricsRepository.GetActiveAlertsAsync(severity, limit);

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active alerts");
                return StatusCode(500, "Internal server error occurred");
            }
        }
    }

    /// <summary>
    /// We manage Kubernetes cluster monitoring through dedicated API endpoints.
    /// Our controller provides comprehensive cluster health and performance data.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EnableRateLimiting("DefaultRateLimitPolicy")]
    [Produces("application/json")]
    public class KubernetesController : ControllerBase
    {
        private readonly IKubernetesMetricsRepository _k8sRepository;
        private readonly ILogger<KubernetesController> _logger;

        /// <summary>
        /// We establish our Kubernetes controller with proper dependency injection.
        /// Our design ensures clean separation of concerns and testability.
        /// </summary>
        public KubernetesController(
            IKubernetesMetricsRepository k8sRepository,
            ILogger<KubernetesController> logger)
        {
            _k8sRepository = k8sRepository ?? throw new ArgumentNullException(nameof(k8sRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// We retrieve current Kubernetes cluster status and resource utilization.
        /// Our endpoint provides real-time cluster health for monitoring dashboards.
        /// </summary>
        /// <param name="clusterName">Optional cluster name filter (default: all clusters)</param>
        /// <returns>Current cluster metrics and status</returns>
        [HttpGet("cluster/status")]
        [ProducesResponseType(typeof(KubernetesClusterStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<KubernetesClusterStatus>> GetClusterStatus(
            [FromQuery] string? clusterName = null)
        {
            try
            {
                _logger.LogDebug("Retrieving cluster status for: {ClusterName}", clusterName ?? "all clusters");

                var status = await _k8sRepository.GetClusterStatusAsync(clusterName);
                
                if (status == null)
                {
                    return NotFound("Cluster not found or no metrics available");
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cluster status");
                return StatusCode(500, "Internal server error occurred");
            }
        }

        /// <summary>
        /// We provide detailed pod information for workload monitoring and troubleshooting.
        /// Our endpoint supports filtering and pagination for efficient data retrieval.
        /// </summary>
        /// <param name="namespace">Kubernetes namespace filter (optional)</param>
        /// <param name="status">Pod status filter (optional)</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Records per page</param>
        /// <returns>Filtered pod information with pagination</returns>
        [HttpGet("pods")]
        [ProducesResponseType(typeof(PagedResult<KubernetesPod>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<KubernetesPod>>> GetPods(
            [FromQuery] string? @namespace = null,
            [FromQuery] string? status = null,
            [FromQuery, Range(1, int.MaxValue)] int page = 1,
            [FromQuery, Range(1, 100)] int pageSize = 20)
        {
            try
            {
                _logger.LogDebug("Retrieving pods for namespace: {Namespace}, status: {Status}", 
                    @namespace, status);

                var result = await _k8sRepository.GetPodsAsync(@namespace, status, page, pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pod information");
                return StatusCode(500, "Internal server error occurred");
            }
        }
    }
}

// src/BTHLCheckGate.WebApi/Program.cs
/**
 * BTHL CheckGate - Web API Startup Configuration
 * File: src/BTHLCheckGate.WebApi/Program.cs
 * 
 * We configure our Web API host with comprehensive security, monitoring, and performance features.
 * Our setup follows enterprise patterns for scalability and maintainability.
 * 
 * @author David St John <davestj@gmail.com>
 * @version 1.0.0
 * @since 2025-09-09
 * 
 * CHANGELOG:
 * 2025-09-09 - FEAT: Initial Web API configuration with enterprise features
 */

using BTHLCheckGate.Data;
using BTHLCheckGate.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

namespace BTHLCheckGate.WebApi
{
    /// <summary>
    /// We establish our Web API program with comprehensive enterprise configuration.
    /// Our setup includes security, monitoring, documentation, and performance optimizations.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// We configure and start our Web API host with all necessary services and middleware.
        /// Our approach ensures proper order of operations and comprehensive feature coverage.
        /// </summary>
        /// <param name="args">Command line arguments for configuration</param>
        /// <returns>Application execution task</returns>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // We configure our database connection with optimized settings
            builder.Services.AddDbContext<CheckGateDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                    mysqlOptions.CommandTimeout(30);
                });
            });

            // We register our repository pattern services for data access
            builder.Services.AddScoped<ISystemMetricsRepository, SystemMetricsRepository>();
            builder.Services.AddScoped<IKubernetesMetricsRepository, KubernetesMetricsRepository>();
            builder.Services.AddScoped<IApiTokenRepository, ApiTokenRepository>();

            // We configure JWT authentication for secure API access
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                });

            // We implement rate limiting for API protection
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("DefaultRateLimitPolicy", rateLimiterOptions =>
                {
                    rateLimiterOptions.PermitLimit = 100;
                    rateLimiterOptions.Window = TimeSpan.FromMinutes(1);
                    rateLimiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    rateLimiterOptions.QueueLimit = 10;
                });
            });

            // We add comprehensive API controllers and services
            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    // We customize model validation responses
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState
                            .Where(x => x.Value!.Errors.Count > 0)
                            .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage));
                        
                        return new BadRequestObjectResult(new { Errors = errors });
                    };
                });

            // We configure API documentation with Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BTHL CheckGate API",
                    Version = "v1.0",
                    Description = "Enterprise System Monitoring REST API",
                    Contact = new OpenApiContact
                    {
                        Name = "BTHL Engineering",
                        Email = "engineering@bthlcorp.com"
                    }
                });

                // We add JWT authentication to Swagger UI
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // We include XML documentation for comprehensive API docs
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            // We configure CORS for web application integration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DefaultCorsPolicy", policy =>
                {
                    policy.WithOrigins("https://localhost:9300", "https://127.0.0.1:9300")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // We add health checks for monitoring and load balancer integration
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<CheckGateDbContext>("database")
                .AddCheck<SystemHealthCheck>("system");

            var app = builder.Build();

            // We configure our middleware pipeline in the correct order
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BTHL CheckGate API v1");
                    options.RoutePrefix = "api/docs";
                });
            }

            // We enforce HTTPS for security
            app.UseHttpsRedirection();

            // We enable CORS for cross-origin requests
            app.UseCors("DefaultCorsPolicy");

            // We apply rate limiting before authentication
            app.UseRateLimiter();

            // We add authentication and authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            // We map our controllers and health check endpoints
            app.MapControllers();
            app.MapHealthChecks("/health");

            // We ensure database is created and migrated
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CheckGateDbContext>();
                await context.Database.EnsureCreatedAsync();
            }

            // We start our application
            await app.RunAsync();
        }
    }
}
