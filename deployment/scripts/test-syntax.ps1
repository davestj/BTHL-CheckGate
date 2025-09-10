# Test PowerShell syntax for all deployment scripts
# File: deployment/scripts/test-syntax.ps1

Write-Host "Testing PowerShell script syntax..." -ForegroundColor Cyan

$scripts = @(
    "deploy-bthl-checkgate.ps1",
    "create-database-fixed.ps1", 
    "verify-setup.ps1",
    "setup.ps1"
)

$allGood = $true

foreach ($script in $scripts) {
    Write-Host "Checking $script..." -ForegroundColor Yellow
    
    try {
        # Test syntax by parsing the script
        $null = [System.Management.Automation.PSParser]::Tokenize((Get-Content $script -Raw), [ref]$null)
        Write-Host "✓ $script syntax is valid" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ $script has syntax errors: $($_.Exception.Message)" -ForegroundColor Red
        $allGood = $false
    }
}

if ($allGood) {
    Write-Host "`nAll scripts passed syntax validation!" -ForegroundColor Green
} else {
    Write-Host "`nSome scripts have syntax errors that need to be fixed." -ForegroundColor Red
}