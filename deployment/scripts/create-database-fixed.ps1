# BTHL CheckGate - Database Setup Script
# File: deployment/scripts/create-database-fixed.ps1

Write-Host "BTHL CheckGate - Database Setup" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan

# MySQL Configuration
$mysqlHost = "localhost"
$mysqlUser = "root" 
$mysqlPassword = "CHANGEME123"
$mysqlPath = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"
$schemaFile = "database/schema/01-initial-schema.sql"

Write-Host "MySQL Configuration:" -ForegroundColor Yellow
Write-Host "Host: $mysqlHost" -ForegroundColor Gray
Write-Host "User: $mysqlUser" -ForegroundColor Gray
Write-Host ""

# Test MySQL connection
Write-Host "Testing MySQL connection..." -ForegroundColor Yellow
try {
    $testQuery = "SELECT VERSION() as version;"
    & "$mysqlPath" -h $mysqlHost -u $mysqlUser -p$mysqlPassword -e $testQuery --silent 2>$null
    $mysqlExitCode = $LASTEXITCODE
    
    if ($mysqlExitCode -eq 0) {
        Write-Host "✓ MySQL connection successful" -ForegroundColor Green
    } else {
        Write-Host "✗ MySQL connection failed (Exit code: $mysqlExitCode)" -ForegroundColor Red
        Write-Host "Please verify:" -ForegroundColor Yellow
        Write-Host "  - MySQL server is running" -ForegroundColor Yellow
        Write-Host "  - Username and password are correct" -ForegroundColor Yellow
        Write-Host "  - Host is accessible" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "✗ Error connecting to MySQL: $_" -ForegroundColor Red
    exit 1
}

# Check schema file exists
if (-not (Test-Path $schemaFile)) {
    Write-Host "✗ Schema file not found: $schemaFile" -ForegroundColor Red
    Write-Host "Please ensure you are running this script from the project root directory" -ForegroundColor Yellow
    exit 1
}

# Create database schema
Write-Host "Creating database and schema..." -ForegroundColor Yellow
try {
    $schemaContent = Get-Content $schemaFile -Raw
    $tempFile = [System.IO.Path]::GetTempFileName() + ".sql"
    $schemaContent | Out-File -FilePath $tempFile -Encoding UTF8
    
    & "$mysqlPath" -h $mysqlHost -u $mysqlUser -p$mysqlPassword < $tempFile 2>$null
    $schemaExitCode = $LASTEXITCODE
    
    Remove-Item $tempFile -ErrorAction SilentlyContinue
    
    if ($schemaExitCode -eq 0) {
        Write-Host "✓ Database schema created successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Database schema creation failed (Exit code: $schemaExitCode)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Error creating database schema: $_" -ForegroundColor Red
    exit 1
}

# Verify database creation
Write-Host "Verifying database setup..." -ForegroundColor Yellow
try {
    $verifyQuery = "USE bthl_checkgate; SHOW TABLES;"
    $tables = & "$mysqlPath" -h $mysqlHost -u $mysqlUser -p$mysqlPassword -e $verifyQuery 2>$null
    $verifyExitCode = $LASTEXITCODE
    
    if ($verifyExitCode -eq 0) {
        Write-Host "✓ Database verification successful" -ForegroundColor Green
        Write-Host "Tables created:" -ForegroundColor Gray
        Write-Host $tables -ForegroundColor Gray
    } else {
        Write-Host "⚠ Database verification failed" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠ Error verifying database: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✓ Database setup completed successfully!" -ForegroundColor Green
Write-Host "You can now run the main setup script or start the application." -ForegroundColor Cyan