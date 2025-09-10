/**
 * BTHL CheckGate - Web API Startup Configuration
 * File: src/BTHLCheckGate.WebApi/Startup.cs
 */

using BTHLCheckGate.Core.Interfaces;
using BTHLCheckGate.Core.Services;
using BTHLCheckGate.Data;
using BTHLCheckGate.Data.Repositories;
using BTHLCheckGate.Security.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

namespace BTHLCheckGate.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Database configuration
            var connectionString = Configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=localhost;Database=bthl_checkgate;Uid=root;Pwd=5243wrvNN;";
            
            services.AddDbContext<CheckGateDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                    mysqlOptions.CommandTimeout(30);
                });
            });

            // Register repositories
            services.AddScoped<ISystemMetricsRepository, SystemMetricsRepository>();
            services.AddScoped<IKubernetesMetricsRepository, KubernetesMetricsRepository>();
            services.AddScoped<IApiTokenRepository, ApiTokenRepository>();

            // Register core services
            services.AddScoped<ISystemMonitoringService, SystemMonitoringService>();
            services.AddScoped<IKubernetesMonitoringService, KubernetesMonitoringService>();
            services.AddScoped<IMetricsCollectionService, MetricsCollectionService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            // JWT Authentication
            var secretKey = Configuration["Jwt:SecretKey"] ?? "BTHLCheckGate-SecretKey-ChangeThis-InProduction-MustBe256BitsOrLonger!";
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"] ?? "BTHLCheckGate",
                        ValidAudience = Configuration["Jwt:Audience"] ?? "BTHLCheckGate-Users",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                });

            // Rate limiting
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("DefaultRateLimitPolicy", rateLimiterOptions =>
                {
                    rateLimiterOptions.PermitLimit = 100;
                    rateLimiterOptions.Window = TimeSpan.FromMinutes(1);
                    rateLimiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    rateLimiterOptions.QueueLimit = 10;
                });
            });

            // API Controllers
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState
                            .Where(x => x.Value!.Errors.Count > 0)
                            .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage));
                        
                        return new BadRequestObjectResult(new { Errors = errors });
                    };
                });

            // Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
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
            });

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultCorsPolicy", policy =>
                {
                    policy.WithOrigins("https://localhost:9300", "https://127.0.0.1:9300")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // Health checks
            services.AddHealthChecks()
                .AddDbContextCheck<CheckGateDbContext>("database");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BTHL CheckGate API v1");
                    options.RoutePrefix = "api/docs";
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("DefaultCorsPolicy");
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            // Ensure database is created
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CheckGateDbContext>();
            context.Database.EnsureCreated();
        }
    }
}