/**
 * BTHL CheckGate - Windows Service Installer
 * File: src/BTHLCheckGate.Service/Services/ServiceInstaller.cs
 */

using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BTHLCheckGate.Service.Services
{
    public static class ServiceInstaller
    {
        public static async Task InstallServiceAsync()
        {
            try
            {
                Console.WriteLine("Installing BTHL CheckGate Windows Service...");
                
                var exePath = Environment.ProcessPath ?? throw new InvalidOperationException("Could not determine executable path");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"create \"BTHLCheckGate\" binPath= \"{exePath} --service\" DisplayName= \"BTHL CheckGate Monitoring Service\" start= auto",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine("Service installed successfully.");
                        Console.WriteLine("Starting service...");
                        
                        // Start the service
                        var startProcess = Process.Start("sc.exe", "start \"BTHLCheckGate\"");
                        if (startProcess != null)
                        {
                            await startProcess.WaitForExitAsync();
                            if (startProcess.ExitCode == 0)
                            {
                                Console.WriteLine("Service started successfully.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to install service: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error installing service: {ex.Message}");
                throw;
            }
        }

        public static async Task UninstallServiceAsync()
        {
            try
            {
                Console.WriteLine("Uninstalling BTHL CheckGate Windows Service...");
                
                // Stop the service first
                var stopProcess = Process.Start("sc.exe", "stop \"BTHLCheckGate\"");
                if (stopProcess != null)
                {
                    await stopProcess.WaitForExitAsync();
                }

                // Delete the service
                var deleteProcess = Process.Start("sc.exe", "delete \"BTHLCheckGate\"");
                if (deleteProcess != null)
                {
                    await deleteProcess.WaitForExitAsync();
                    if (deleteProcess.ExitCode == 0)
                    {
                        Console.WriteLine("Service uninstalled successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uninstalling service: {ex.Message}");
                throw;
            }
        }
    }
}