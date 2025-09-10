/**
 * BTHL CheckGate - Web API Host Service
 * File: src/BTHLCheckGate.Service/Services/WebApiHostService.cs
 */

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BTHLCheckGate.WebApi;

namespace BTHLCheckGate.Service.Services
{
    public class WebApiHostService : BackgroundService
    {
        private readonly ILogger<WebApiHostService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IWebHost? _webHost;

        public WebApiHostService(ILogger<WebApiHostService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting Web API host service on port 9300");

                _webHost = new WebHostBuilder()
                    .UseKestrel(options =>
                    {
                        options.ListenAnyIP(9300, listenOptions =>
                        {
                            listenOptions.UseHttps();
                        });
                    })
                    .UseStartup<Startup>()
                    .ConfigureServices(services =>
                    {
                        // Copy services from main service provider
                        foreach (var service in _serviceProvider.GetServices<ServiceDescriptor>())
                        {
                            services.Add(service);
                        }
                    })
                    .Build();

                await _webHost.StartAsync(stoppingToken);
                _logger.LogInformation("Web API host service started successfully");

                // Keep running until cancellation is requested
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Web API host service is stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Web API host service");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Web API host service");
            
            if (_webHost != null)
            {
                await _webHost.StopAsync(cancellationToken);
                _webHost.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}