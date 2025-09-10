# BTHL CheckGate - Complete Setup Script
# File: deployment/scripts/setup.ps1

Write-Host "BTHL CheckGate - Enterprise Setup Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "Please run this script as Administrator" -ForegroundColor Red
    exit 1
}

# MySQL Configuration
$mysqlHost = "localhost"
$mysqlUser = "root" 
$mysqlPassword = "CHANGEME123"
$mysqlPath = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"

Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check .NET 8 installation
Write-Host "Checking .NET 8..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($dotnetVersion -like "8.*") {
        Write-Host "✓ .NET 8 installed: $dotnetVersion" -ForegroundColor Green
    } else {
        Write-Host "✗ .NET 8 not found. Current version: $dotnetVersion" -ForegroundColor Red
        Write-Host "Please install .NET 8 from https://dotnet.microsoft.com/download" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "✗ .NET not installed. Please install .NET 8 from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Check Docker installation  
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version 2>$null
    Write-Host "✓ Docker installed: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "⚠ Docker not found. Kubernetes monitoring will be limited." -ForegroundColor Yellow
    Write-Host "Install Docker Desktop from https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
}

# Check MySQL installation
Write-Host "Checking MySQL..." -ForegroundColor Yellow
try {
    $mysqlVersion = mysql --version 2>$null
    Write-Host "✓ MySQL installed: $mysqlVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ MySQL not found. Please install MySQL 8.0" -ForegroundColor Red
    exit 1
}

# Set up MySQL database
Write-Host "Setting up MySQL database..." -ForegroundColor Yellow
try {
    # Test MySQL connection first
    Write-Host "Testing MySQL connection..." -ForegroundColor Yellow
    $testQuery = "SELECT VERSION();"
    & "$mysqlPath" -h $mysqlHost -u $mysqlUser -p$mysqlPassword -e $testQuery 2>$null
    $result = $LASTEXITCODE
    
    if ($result -eq 0) {
        Write-Host "✓ MySQL connection successful" -ForegroundColor Green
        
        # Create database and run schema
        Write-Host "Creating database schema..." -ForegroundColor Yellow
        $schemaPath = "database/schema/01-initial-schema.sql"
        if (Test-Path $schemaPath) {
            & "$mysqlPath" -h $mysqlHost -u $mysqlUser -p$mysqlPassword < $schemaPath
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Database schema created successfully" -ForegroundColor Green
            } else {
                Write-Host "✗ Database schema creation failed" -ForegroundColor Red
            }
        } else {
            Write-Host "✗ Schema file not found: $schemaPath" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ MySQL connection failed. Check credentials and server status." -ForegroundColor Red
        Write-Host "Host: $mysqlHost, User: $mysqlUser" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "✗ Database setup failed: $_" -ForegroundColor Red
    exit 1
}

# Build the application
Write-Host "Building application..." -ForegroundColor Yellow
try {
    dotnet build BTHLCheckGate.sln --configuration Release
    Write-Host "✓ Build completed successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Build failed: $_" -ForegroundColor Red
}

# Install as Windows Service
Write-Host "Installing Windows Service..." -ForegroundColor Yellow
try {
    dotnet run --project src/BTHLCheckGate.Service -- --install
    Write-Host "✓ Service installed successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Service installation failed: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "Setup completed! Access the application at https://localhost:9300" -ForegroundColor Cyan
Write-Host "API documentation: https://localhost:9300/api/docs" -ForegroundColor Cyan