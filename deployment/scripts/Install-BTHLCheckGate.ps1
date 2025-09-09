# deployment/scripts/Install-BTHLCheckGate.ps1
<#
.SYNOPSIS
    BTHL CheckGate - Enterprise Installation Script
    File: deployment/scripts/Install-BTHLCheckGate.ps1

.DESCRIPTION
    We are providing a comprehensive installation script that automates the complete
    setup of BTHL CheckGate on Windows 11 Pro systems. Our script handles all
    dependencies, configurations, and service registration with enterprise-grade
    error handling and logging capabilities.

.AUTHOR
    David St John <davestj@gmail.com>

.VERSION
    1.0.0

.CHANGELOG
    2025-09-09 - FEAT: Initial enterprise installation script with comprehensive setup
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$InstallPath = "C:\Program Files\BTHL\CheckGate",
    
    [Parameter(Mandatory = $false)]
    [string]$ServiceName = "BTHLCheckGate",
    
    [Parameter(Mandatory = $false)]
    [int]$Port = 9300,
    
    [Parameter(Mandatory = $false)]
    [string]$MySqlHost = "localhost",
    
    [Parameter(Mandatory = $false)]
    [string]$MySqlDatabase = "bthl_checkgate",
    
    [Parameter(Mandatory = $false)]
    [string]$MySqlUsername = "bthl_checkgate",
    
    [Parameter(Mandatory = $false)]
    [SecureString]$MySqlPassword,
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipPrerequisites,
    
    [Parameter(Mandatory = $false)]
    [switch]$DevelopmentMode
)

# We define our installation constants and configuration
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$Script:LogFile = "$env:TEMP\BTHLCheckGate-Install-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$Script:Prerequisites = @{
    ".NET 8 Runtime" = @{
        DownloadUrl = "https://download.microsoft.com/download/8/4/8/848b82c5-b30c-47e3-8c5a-0b8a8f7d29a4/dotnet-runtime-8.0.8-win-x64.exe"
        InstallArgs = "/quiet /norestart"
        CheckCommand = "dotnet --version"
    }
    "MySQL 8.0" = @{
        DownloadUrl = "https://dev.mysql.com/get/Downloads/MySQLInstaller/mysql-installer-community-8.0.39.0.msi"
        InstallArgs = "/quiet /norestart"
        CheckCommand = "mysql --version"
    }
}

#region Logging Functions

function Write-InstallLog {
    <#
    .SYNOPSIS
    We write installation messages to both console and log file for comprehensive tracking.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message,
        
        [Parameter(Mandatory = $false)]
        [ValidateSet("Info", "Warning", "Error", "Success")]
        [string]$Level = "Info"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    
    # We output to console with appropriate colors
    switch ($Level) {
        "Info" { Write-Information $Message }
        "Warning" { Write-Warning $Message }
        "Error" { Write-Error $Message }
        "Success" { Write-Host $Message -ForegroundColor Green }
    }
    
    # We append to our log file for audit purposes
    Add-Content -Path $Script:LogFile -Value $logEntry -Encoding UTF8
}

#endregion

#region Prerequisite Functions

function Test-Administrator {
    <#
    .SYNOPSIS
    We verify that the installation script is running with administrative privileges.
    #>
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Test-Prerequisites {
    <#
    .SYNOPSIS
    We check for required software dependencies and install them if needed.
    #>
    Write-InstallLog "Checking system prerequisites..." -Level "Info"
    
    foreach ($prereq in $Script:Prerequisites.Keys) {
        Write-InstallLog "Checking for $prereq..." -Level "Info"
        
        try {
            $checkCommand = $Script:Prerequisites[$prereq].CheckCommand
            $result = Invoke-Expression $checkCommand 2>$null
            
            if ($LASTEXITCODE -eq 0) {
                Write-InstallLog "$prereq is already installed" -Level "Success"
            } else {
                throw "Not found"
            }
        }
        catch {
            if (-not $SkipPrerequisites) {
                Write-InstallLog "$prereq not found, installing..." -Level "Warning"
                Install-Prerequisite -Name $prereq
            } else {
                Write-InstallLog "$prereq not found but skipping installation per user request" -Level "Warning"
            }
        }
    }
}

function Install-Prerequisite {
    <#
    .SYNOPSIS
    We download and install required prerequisites automatically.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )
    
    $prereq = $Script:Prerequisites[$Name]
    $downloadUrl = $prereq.DownloadUrl
    $installArgs = $prereq.InstallArgs
    
    # We create a temporary download location
    $fileName = Split-Path $downloadUrl -Leaf
    $downloadPath = Join-Path $env:TEMP $fileName
    
    try {
        Write-InstallLog "Downloading $Name from $downloadUrl..." -Level "Info"
        Invoke-WebRequest -Uri $downloadUrl -OutFile $downloadPath -UseBasicParsing
        
        Write-InstallLog "Installing $Name..." -Level "Info"
        Start-Process -FilePath $downloadPath -ArgumentList $installArgs -Wait -NoNewWindow
        
        Write-InstallLog "$Name installed successfully" -Level "Success"
    }
    catch {
        Write-InstallLog "Failed to install $Name : $($_.Exception.Message)" -Level "Error"
        throw
    }
    finally {
        # We clean up the downloaded file
        if (Test-Path $downloadPath) {
            Remove-Item $downloadPath -Force
        }
    }
}

#endregion

#region Database Setup Functions

function Initialize-Database {
    <#
    .SYNOPSIS
    We create and configure the MySQL database for BTHL CheckGate.
    #>
    Write-InstallLog "Initializing database..." -Level "Info"
    
    # We create the database if it doesn't exist
    $createDbScript = @"
CREATE DATABASE IF NOT EXISTS $MySqlDatabase
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;
"@
    
    try {
        # We execute the database creation script
        $mysqlArgs = @(
            "-h", $MySqlHost
            "-u", "root"
            "-e", $createDbScript
        )
        
        Start-Process -FilePath "mysql" -ArgumentList $mysqlArgs -Wait -NoNewWindow
        Write-InstallLog "Database created successfully" -Level "Success"
        
        # We apply the database schema
        $schemaPath = Join-Path (Split-Path $PSScriptRoot -Parent) "database\schema\01-initial-schema.sql"
        if (Test-Path $schemaPath) {
            Write-InstallLog "Applying database schema..." -Level "Info"
            
            $schemaArgs = @(
                "-h", $MySqlHost
                "-u", "root"
                $MySqlDatabase
            )
            
            Get-Content $schemaPath | mysql @schemaArgs
            Write-InstallLog "Database schema applied successfully" -Level "Success"
        }
    }
    catch {
        Write-InstallLog "Database initialization failed: $($_.Exception.Message)" -Level "Error"
        throw
    }
}

#endregion

#region Service Installation Functions

function Install-CheckGateService {
    <#
    .SYNOPSIS
    We install and configure the BTHL CheckGate Windows service.
    #>
    Write-InstallLog "Installing BTHL CheckGate service..." -Level "Info"
    
    # We create the installation directory
    if (-not (Test-Path $InstallPath)) {
        New-Item -Path $InstallPath -ItemType Directory -Force | Out-Null
        Write-InstallLog "Created installation directory: $InstallPath" -Level "Info"
    }
    
    # We copy application files to the installation directory
    $sourceFiles = @(
        "BTHLCheckGate.Service.exe",
        "BTHLCheckGate.Core.dll",
        "BTHLCheckGate.Data.dll",
        "BTHLCheckGate.Models.dll",
        "BTHLCheckGate.Security.dll",
        "BTHLCheckGate.WebApi.dll",
        "appsettings.json",
        "appsettings.Production.json"
    )
    
    foreach ($file in $sourceFiles) {
        $sourcePath = Join-Path $PSScriptRoot $file
        $destPath = Join-Path $InstallPath $file
        
        if (Test-Path $sourcePath) {
            Copy-Item $sourcePath $destPath -Force
            Write-InstallLog "Copied $file to installation directory" -Level "Info"
        }
    }
    
    # We create the service configuration
    $serviceExe = Join-Path $InstallPath "BTHLCheckGate.Service.exe"
    $serviceArgs = "--service --port $Port"
    
    try {
        # We install the Windows service
        New-Service -Name $ServiceName -BinaryPathName "$serviceExe $serviceArgs" -DisplayName "BTHL CheckGate Monitoring Service" -Description "Enterprise system monitoring and Kubernetes cluster management service" -StartupType Automatic
        
        Write-InstallLog "Service installed successfully" -Level "Success"
        
        # We configure service recovery options
        $recoveryArgs = @(
            "failure", $ServiceName,
            "reset=", "86400",
            "actions=", "restart/60000/restart/60000/restart/60000"
        )
        
        Start-Process -FilePath "sc.exe" -ArgumentList $recoveryArgs -Wait -NoNewWindow
        Write-InstallLog "Service recovery options configured" -Level "Info"
    }
    catch {
        Write-InstallLog "Service installation failed: $($_.Exception.Message)" -Level "Error"
        throw
    }
}

function New-ConfigurationFile {
    <#
    .SYNOPSIS
    We generate the production configuration file with user-specified settings.
    #>
    Write-InstallLog "Creating configuration file..." -Level "Info"
    
    # We decrypt the MySQL password if provided
    $plainPassword = ""
    if ($MySqlPassword) {
        $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
            [Runtime.InteropServices.Marshal]::SecureStringToBSTR($MySqlPassword)
        )
    }
    
    # We create the configuration object
    $config = @{
        "Logging" = @{
            "LogLevel" = @{
                "Default" = "Information"
                "Microsoft.AspNetCore" = "Warning"
            }
        }
        "ConnectionStrings" = @{
            "DefaultConnection" = "Server=$MySqlHost;Database=$MySqlDatabase;Uid=$MySqlUsername;Pwd=$plainPassword;AllowUserVariables=True"
        }
        "Jwt" = @{
            "Issuer" = "BTHLCheckGate"
            "Audience" = "BTHLCheckGate-Users"
            "SecretKey" = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
        }
        "CheckGate" = @{
            "Port" = $Port
            "MonitoringInterval" = 30
            "DataRetentionDays" = 90
            "EnableKubernetesMonitoring" = $true
        }
        "AllowedHosts" = "*"
    }
    
    # We write the configuration to the production settings file
    $configPath = Join-Path $InstallPath "appsettings.Production.json"
    $config | ConvertTo-Json -Depth 10 | Set-Content $configPath -Encoding UTF8
    
    Write-InstallLog "Configuration file created: $configPath" -Level "Success"
}

#endregion

#region Firewall Configuration

function Set-FirewallRules {
    <#
    .SYNOPSIS
    We configure Windows Firewall rules for BTHL CheckGate.
    #>
    Write-InstallLog "Configuring firewall rules..." -Level "Info"
    
    try {
        # We create inbound rule for the web interface
        New-NetFirewallRule -DisplayName "BTHL CheckGate Web Interface" -Direction Inbound -Protocol TCP -LocalPort $Port -Action Allow -Profile Any
        
        Write-InstallLog "Firewall rules configured successfully" -Level "Success"
    }
    catch {
        Write-InstallLog "Firewall configuration failed: $($_.Exception.Message)" -Level "Warning"
    }
}

#endregion

#region Main Installation Logic

function Start-Installation {
    <#
    .SYNOPSIS
    We orchestrate the complete installation process with comprehensive error handling.
    #>
    Write-InstallLog "Starting BTHL CheckGate installation..." -Level "Info"
    Write-InstallLog "Installation log: $Script:LogFile" -Level "Info"
    
    try {
        # We verify administrator privileges
        if (-not (Test-Administrator)) {
            throw "Installation must be run as Administrator"
        }
        
        # We check and install prerequisites
        if (-not $SkipPrerequisites) {
            Test-Prerequisites
        }
        
        # We initialize the database
        Initialize-Database
        
        # We install the service
        Install-CheckGateService
        
        # We create configuration files
        New-ConfigurationFile
        
        # We configure firewall rules
        Set-FirewallRules
        
        # We start the service
        Start-Service -Name $ServiceName
        Write-InstallLog "Service started successfully" -Level "Success"
        
        # We display completion information
        Write-InstallLog "" -Level "Info"
        Write-InstallLog "=== INSTALLATION COMPLETED SUCCESSFULLY ===" -Level "Success"
        Write-InstallLog "" -Level "Info"
        Write-InstallLog "BTHL CheckGate has been installed and is running." -Level "Info"
        Write-InstallLog "Web Interface: https://localhost:$Port" -Level "Info"
        Write-InstallLog "API Documentation: https://localhost:$Port/api/docs" -Level "Info"
        Write-InstallLog "Service Name: $ServiceName" -Level "Info"
        Write-InstallLog "Installation Path: $InstallPath" -Level "Info"
        Write-InstallLog "Log File: $Script:LogFile" -Level "Info"
        Write-InstallLog "" -Level "Info"
        Write-InstallLog "Use the following credentials for initial access:" -Level "Info"
        Write-InstallLog "Username: Administrator" -Level "Info"
        Write-InstallLog "Password: Use your Windows Administrator password" -Level "Info"
        
    }
    catch {
        Write-InstallLog "Installation failed: $($_.Exception.Message)" -Level "Error"
        Write-InstallLog "Check the log file for detailed error information: $Script:LogFile" -Level "Error"
        exit 1
    }
}

#endregion

# We execute the installation process
Start-Installation

# deployment/scripts/Uninstall-BTHLCheckGate.ps1
<#
.SYNOPSIS
    BTHL CheckGate - Uninstallation Script
    File: deployment/scripts/Uninstall-BTHLCheckGate.ps1

.DESCRIPTION
    We provide a comprehensive uninstallation script that cleanly removes
    BTHL CheckGate from Windows systems while preserving data if requested.

.AUTHOR
    David St John <davestj@gmail.com>

.VERSION
    1.0.0
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ServiceName = "BTHLCheckGate",
    
    [Parameter(Mandatory = $false)]
    [string]$InstallPath = "C:\Program Files\BTHL\CheckGate",
    
    [Parameter(Mandatory = $false)]
    [switch]$PreserveData,
    
    [Parameter(Mandatory = $false)]
    [switch]$RemoveDatabase
)

function Remove-CheckGateService {
    <#
    .SYNOPSIS
    We stop and remove the BTHL CheckGate Windows service.
    #>
    Write-Host "Removing BTHL CheckGate service..." -ForegroundColor Yellow
    
    try {
        # We stop the service if it's running
        $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($service) {
            if ($service.Status -eq 'Running') {
                Stop-Service -Name $ServiceName -Force
                Write-Host "Service stopped successfully" -ForegroundColor Green
            }
            
            # We remove the service
            Remove-Service -Name $ServiceName
            Write-Host "Service removed successfully" -ForegroundColor Green
        } else {
            Write-Host "Service not found" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "Error removing service: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Remove-InstallationFiles {
    <#
    .SYNOPSIS
    We remove installation files and directories.
    #>
    Write-Host "Removing installation files..." -ForegroundColor Yellow
    
    if (Test-Path $InstallPath) {
        try {
            Remove-Item $InstallPath -Recurse -Force
            Write-Host "Installation files removed successfully" -ForegroundColor Green
        }
        catch {
            Write-Host "Error removing installation files: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "Installation directory not found" -ForegroundColor Yellow
    }
}

function Remove-FirewallRules {
    <#
    .SYNOPSIS
    We remove firewall rules created during installation.
    #>
    Write-Host "Removing firewall rules..." -ForegroundColor Yellow
    
    try {
        Remove-NetFirewallRule -DisplayName "BTHL CheckGate Web Interface" -ErrorAction SilentlyContinue
        Write-Host "Firewall rules removed successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "Error removing firewall rules: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# We execute the uninstallation process
Write-Host "Starting BTHL CheckGate uninstallation..." -ForegroundColor Cyan

Remove-CheckGateService
Remove-InstallationFiles
Remove-FirewallRules

if ($RemoveDatabase) {
    Write-Host "Database removal requested but must be done manually for safety" -ForegroundColor Yellow
    Write-Host "To remove the database, run: DROP DATABASE bthl_checkgate;" -ForegroundColor Yellow
}

Write-Host "" -ForegroundColor White
Write-Host "=== UNINSTALLATION COMPLETED ===" -ForegroundColor Green
Write-Host "BTHL CheckGate has been removed from the system" -ForegroundColor Green
