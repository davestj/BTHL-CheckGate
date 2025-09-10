# BTHL CheckGate - Setup Verification Script
# File: deployment/scripts/verify-setup.ps1

Write-Host "BTHL CheckGate - Setup Verification" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan

$allGood = $true

# Check .NET 8
Write-Host "Checking .NET 8..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($dotnetVersion -like "8.*") {
        Write-Host "✓ .NET 8 installed: $dotnetVersion" -ForegroundColor Green
    } else {
        Write-Host "✗ .NET 8 not found. Current: $dotnetVersion" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "✗ .NET not installed" -ForegroundColor Red
    $allGood = $false
}

# Check MySQL
Write-Host "Checking MySQL..." -ForegroundColor Yellow
try {
    $testQuery = "SELECT VERSION();"
    & "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -h localhost -u root -pCHANGEME123 -e $testQuery 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ MySQL connection successful" -ForegroundColor Green
    } else {
        Write-Host "✗ MySQL connection failed" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "✗ MySQL not accessible" -ForegroundColor Red
    $allGood = $false
}

# Check Database
Write-Host "Checking BTHL CheckGate database..." -ForegroundColor Yellow
try {
    $dbQuery = "USE bthl_checkgate; SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'bthl_checkgate';"
    $tableCount = & "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -h localhost -u root -pCHANGEME123 -e $dbQuery --silent 2>$null
    if ($LASTEXITCODE -eq 0 -and $tableCount -gt 0) {
        Write-Host "✓ Database exists with $tableCount tables" -ForegroundColor Green
    } else {
        Write-Host "✗ Database not properly created" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "✗ Database verification failed" -ForegroundColor Red
    $allGood = $false
}

# Check Docker (optional)
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Docker installed: $dockerVersion" -ForegroundColor Green
    } else {
        Write-Host "⚠ Docker not found (Kubernetes monitoring will be limited)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠ Docker not accessible" -ForegroundColor Yellow
}

# Check if solution builds
Write-Host "Checking if solution builds..." -ForegroundColor Yellow
try {
    dotnet build BTHLCheckGate.sln --verbosity quiet --nologo 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Solution builds successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Solution build failed" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "✗ Build verification failed" -ForegroundColor Red
    $allGood = $false
}

Write-Host ""
if ($allGood) {
    Write-Host "🎉 All checks passed! BTHL CheckGate is ready to run." -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Run: dotnet run --project src/BTHLCheckGate.Service -- --console" -ForegroundColor White
    Write-Host "2. Open: https://localhost:9300" -ForegroundColor White
    Write-Host "3. API docs: https://localhost:9300/api/docs" -ForegroundColor White
} else {
    Write-Host "❌ Some checks failed. Please resolve the issues above." -ForegroundColor Red
}