# BTHL CheckGate - Complete Deployment Script
# File: deployment/scripts/deploy-bthl-checkgate.ps1

Write-Host "BTHL CheckGate - Complete Deployment" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "Please run this script as Administrator" -ForegroundColor Red
    exit 1
}

# Configuration
$mysqlHost = "localhost"
$mysqlUser = "root" 
$mysqlPassword = "CHANGEME123"
$mysqlPath = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"
$schemaFile = "database\schema\01-initial-schema.sql"

Write-Host "Starting deployment process..." -ForegroundColor Yellow

# Check .NET 9
Write-Host "Checking .NET 9..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version 2>$null
    $versionNumber = [Version]($dotnetVersion.Split('-')[0])
    if ($versionNumber.Major -ge 9) {
        Write-Host "OK .NET $($versionNumber.Major) installed: $dotnetVersion (compatible)" -ForegroundColor Green
    } else {
        Write-Host "ERROR .NET 9+ not found. Current version: $dotnetVersion" -ForegroundColor Red
        Write-Host "Please install .NET 9 or higher from https://dotnet.microsoft.com/download" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "ERROR .NET not installed. Please install .NET 9 from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Check Docker (optional)
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    $dockerResult = docker --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK Docker installed: $dockerResult" -ForegroundColor Green
    } else {
        Write-Host "WARNING Docker not found. Kubernetes monitoring will be limited." -ForegroundColor Yellow
    }
} catch {
    Write-Host "WARNING Docker not accessible. Kubernetes monitoring will be limited." -ForegroundColor Yellow
}

# Check MySQL
Write-Host "Checking MySQL..." -ForegroundColor Yellow
if (Test-Path $mysqlPath) {
    Write-Host "OK MySQL found at: $mysqlPath" -ForegroundColor Green
} else {
    Write-Host "ERROR MySQL not found at expected path: $mysqlPath" -ForegroundColor Red
    exit 1
}

# Test MySQL connection
Write-Host "Testing MySQL connection..." -ForegroundColor Yellow
try {
    $testArgs = @(
        "-h", $mysqlHost,
        "-u", $mysqlUser,
        "-p$mysqlPassword",
        "-e", "SELECT 1",
        "--silent"
    )
    
    $result = Start-Process -FilePath $mysqlPath -ArgumentList $testArgs -Wait -NoNewWindow -PassThru
    $mysqlExitCode = $result.ExitCode
    
    if ($mysqlExitCode -eq 0) {
        Write-Host "OK MySQL connection successful" -ForegroundColor Green
    } else {
        Write-Host "ERROR MySQL connection failed (Exit code: $mysqlExitCode)" -ForegroundColor Red
        Write-Host "Please verify:" -ForegroundColor Yellow
        Write-Host "  - MySQL server is running" -ForegroundColor Yellow
        Write-Host "  - Username and password are correct" -ForegroundColor Yellow
        Write-Host "  - Host is accessible" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "ERROR connecting to MySQL: $_" -ForegroundColor Red
    exit 1
}

# Check schema file exists
if (-not (Test-Path $schemaFile)) {
    Write-Host "ERROR Schema file not found: $schemaFile" -ForegroundColor Red
    Write-Host "Please ensure you are running this script from the project root directory" -ForegroundColor Yellow
    exit 1
}

# Create database schema
Write-Host "Creating database and schema..." -ForegroundColor Yellow
try {
    $schemaContent = Get-Content $schemaFile -Raw
    $tempFile = [System.IO.Path]::GetTempFileName() + ".sql"
    $schemaContent | Out-File -FilePath $tempFile -Encoding UTF8
    
    $schemaArgs = @(
        "-h", $mysqlHost,
        "-u", $mysqlUser,
        "-p$mysqlPassword"
    )
    
    $result = Start-Process -FilePath $mysqlPath -ArgumentList $schemaArgs -Wait -NoNewWindow -PassThru -RedirectStandardInput $tempFile
    $schemaExitCode = $result.ExitCode
    
    Remove-Item $tempFile -ErrorAction SilentlyContinue
    
    if ($schemaExitCode -eq 0) {
        Write-Host "OK Database schema created successfully" -ForegroundColor Green
    } else {
        Write-Host "ERROR Database schema creation failed (Exit code: $schemaExitCode)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "ERROR creating database schema: $_" -ForegroundColor Red
    exit 1
}

# Verify database creation
Write-Host "Verifying database setup..." -ForegroundColor Yellow
try {
    $verifyArgs = @(
        "-h", $mysqlHost,
        "-u", $mysqlUser,
        "-p$mysqlPassword",
        "-e", "USE bthl_checkgate; SHOW TABLES;",
        "--silent"
    )
    
    $verifyResult = Start-Process -FilePath $mysqlPath -ArgumentList $verifyArgs -Wait -NoNewWindow -PassThru -RedirectStandardOutput "verify_output.txt"
    $verifyExitCode = $verifyResult.ExitCode
    
    if ($verifyExitCode -eq 0) {
        Write-Host "OK Database verification successful" -ForegroundColor Green
        if (Test-Path "verify_output.txt") {
            $tables = Get-Content "verify_output.txt"
            Write-Host "Tables created:" -ForegroundColor Gray
            Write-Host $tables -ForegroundColor Gray
            Remove-Item "verify_output.txt" -ErrorAction SilentlyContinue
        }
    } else {
        Write-Host "WARNING Database verification failed" -ForegroundColor Yellow
    }
} catch {
    Write-Host "WARNING Error verifying database: $_" -ForegroundColor Yellow
}

# Build the solution
Write-Host "Building solution..." -ForegroundColor Yellow
try {
    dotnet build BTHLCheckGate.sln --configuration Release --verbosity minimal
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK Solution built successfully" -ForegroundColor Green
    } else {
        Write-Host "ERROR Solution build failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "ERROR Build failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "SUCCESS BTHL CheckGate deployment completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Run service: dotnet run --project src/BTHLCheckGate.Service -- --console" -ForegroundColor White
Write-Host "2. Open dashboard: https://localhost:9300" -ForegroundColor White
Write-Host "3. View API docs: https://localhost:9300/api/docs" -ForegroundColor White
Write-Host "4. Test with client: .\client\test-client\BTHLCheckGate.TestClient.exe --interactive" -ForegroundColor White